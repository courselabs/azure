using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace FulfilmentProcessor;

public class Worker : BackgroundService
{
    private static Random _Random = new Random();
    private readonly TelemetryClient _telemetry;
    private readonly IConfiguration _config; 
    private readonly ILogger<Worker> _logger;

    public Worker(TelemetryClient telemetry, IConfiguration config, ILogger<Worker> logger)
    {
        _telemetry = telemetry;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var failureFactor = _config.GetValue<double>("Fulfilment:FailureFactor");
        while (!stoppingToken.IsCancellationRequested)
        {
            var inFlight = GenerateMetric(0, 200);
            var failed = _Random.Next(0, (int)Math.Round(inFlight * failureFactor));
            using (var operation = _telemetry.StartOperation<RequestTelemetry>("BatchReceived"))
            {
                RecordProcessed(inFlight, failed);
                RecordFailed(inFlight, failed);

                operation.Telemetry.ResponseCode = "200";
                _telemetry.StopOperation(operation);
            }
            var waitMs = _Random.Next(1, 20) * 1000;
            await Task.Delay(waitMs, stoppingToken);
        }
    }

    private int GenerateMetric(int from, int to)
    {
        return _Random.Next(from, to);
    }

    private void RecordProcessed(int processed, int failed)
    {
        for (int i = 0; i < processed - failed; i++)
        {
            var requestId = _Random.Next(20000000, 40000000);
            var duration = _Random.Next(2000, 12000);
            _logger.LogTrace("{EventType}: Request ID: {RequestId}", EventType.Requested, requestId);
            _logger.LogDebug("{EventType}: Request ID: {RequestId}", EventType.InFlight, requestId);
            _logger.LogInformation("{EventType}: Request ID: {RequestId}. Took: {Duration}ms.", EventType.Processed, requestId, duration);
            _telemetry.TrackEvent(EventType.Processed, new Dictionary<string, string>() { { "RequestID", $"{requestId}" } });
            _telemetry.TrackDependency("DocumentService", "Print_Document", $"{requestId}", DateTimeOffset.UtcNow.AddMilliseconds(-1 * duration), TimeSpan.FromMilliseconds(duration), true);
        }
        _telemetry.TrackMetric("QueueSize", _Random.Next(0, 50000));
    }

    private void RecordFailed(int processed, int failed)
    {
        for (int i = 0; i < failed; i++)
        {
            var requestId = _Random.Next(30000000, 35000000);

            var errorMessage = ErrorMessage.Unavailable;
            if (i == 15 && processed > 150)
            {
                errorMessage = ErrorMessage.NoPaper;
            }
            else if (i > 10 && processed > 100)
            {
                errorMessage = ErrorMessage.Code302;
                _telemetry.TrackDependency("DocumentService", "Print_Document", $"{requestId}", DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(0), false);
            }
            _logger.LogTrace("{EventType}: Request ID: {RequestId}", EventType.Requested, requestId);
            _logger.LogError("{EventType}: Request ID: {RequestId}. Error: {ErrorMessage}", EventType.Failed, requestId, errorMessage);
            _telemetry.TrackEvent(EventType.Failed, new Dictionary<string, string>() { { "RequestID", $"{requestId}" } });
        }
    }

    private struct EventType
    {
        public const string Processed = "Fulfilment.Processed";
        public const string Failed = "Fulfilment.Failed";
        public const string Requested = "Fulfilment.Requested";
        public const string InFlight = "Fulfilment.InFlight";
    }

    private struct ErrorMessage
    {
        public const string Unavailable = "Document service unavailable";
        public const string Code302 = "Document service error code 302";
        public const string NoPaper = "Out of paper. Please load plain A4 into tray 1";
    }
}
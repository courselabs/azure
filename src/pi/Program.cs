using PowerArgs;
using StackExchange.Redis;

namespace Pi;

class Program
{
    private const string CHANNEL = "events.pi.computed";

    private static Arguments Arguments;
    private static ConnectionMultiplexer Redis;

    static void Main(string[] args)
    {
        Arguments = Args.Parse<Arguments>(args);
        if (!string.IsNullOrEmpty(Arguments.ConnectionString))
        {
            Redis = ConnectionMultiplexer.Connect(Arguments.ConnectionString);
            DebugLog("Connected to Redis");
        }

        string pi;
        if (Arguments.UseCache && Redis != null)
        {
            var db = Redis.GetDatabase();
            var key = $"pi-{Arguments.DecimalPlaces}";
            pi = db.StringGet(key);
            if (string.IsNullOrEmpty(pi))
            {
                pi = GetPi();
                db.StringSet(key, pi);
                DebugLog($"Calculation added to cache with key: {key}");
            }
            else
            {
                 DebugLog($"Fetched calculation from cache with key: {key}");
            }
        }
        else
        {
            pi = GetPi();
        }

        Console.WriteLine(pi);
    }

    static string GetPi()
    {
        var pi = MachinFormula.Calculate(Arguments.DecimalPlaces).ToString();
        DebugLog("Calculated Pi");
        if (Arguments.PublishEvents && Redis != null)
        {
            var subscriber = Redis.GetSubscriber();
            subscriber.Publish(CHANNEL, $"Calculated Pi to: {Arguments.DecimalPlaces}dp");
            DebugLog($"Published calculation event to channel: {CHANNEL}");
        }
        return pi;
    }

    private static void DebugLog(string log)
    {
        if (Arguments.Debug)
        {
            Console.WriteLine($"--{log}");
        }
    }
}
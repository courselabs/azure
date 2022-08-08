using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Numbers.Api.Controllers
{
    [ApiController]
    [Route("rng")]
    public class RngController : ControllerBase
    {
        private static Random _Random = new Random();
        private static int _CallCount;

        private readonly ILogger<RngController> _logger;
        private readonly IConfiguration _config;
        private readonly int _failAfterCallCount;
        private readonly bool _useFailureId;
        private readonly string _instance;

        public RngController(IConfiguration config, ILogger<RngController> logger)
        {
            _config = config;
            _logger = logger;
            _failAfterCallCount = _config.GetValue<int>("FailAfterCallCount");
            _useFailureId = _config.GetValue<bool>("UseFailureId");
            _instance = Dns.GetHostName();
        }

        [HttpGet]
        public IActionResult Get()
        {
            _CallCount++;
            
            if (Status.Healthy)
            {
                var n = _Random.Next(0, 100);
                _logger.LogDebug($"Instance: {_instance}. Returning random number: {n}");

                if (_failAfterCallCount > 0 && _CallCount >= _failAfterCallCount)
                {
                    Status.Healthy = false;
                }

                return Ok(n);
            }
            else
            {
                var message = _useFailureId ? $"Instance: {_instance}. Unhealthy! Failure ID: {Guid.NewGuid()}" : $"Instance: {_instance}. Unhealthy!";
                _logger.LogWarning(message);
                return StatusCode(500, new { message= message });
            }
        }
    }
}

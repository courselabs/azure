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
        private readonly string _instance;

        public RngController(IConfiguration config, ILogger<RngController> logger)
        {
            _config = config;
            _logger = logger;
            _instance = Dns.GetHostName();

            if (_CallCount == 0)
            {
                _logger.LogInformation("Random number generator initialized");
            }
        }

        [HttpGet]
        [ProducesResponseType(typeof(int), 200)]
        [ProducesResponseType(400)]
        public IActionResult Get(int min, int max)
        {
            _CallCount++;

            if (Status.Healthy)
            {
                var configMin = _config.GetValue<int>("Rng:Range:Min");
                var configMax = _config.GetValue<int>("Rng:Range:Max");

                if (max <= min || (min < configMin) || (max > configMax))
                {
                    _logger.LogInformation($"User request invalid; min: {min}; max: {max}; configured min: {configMin}; max: {configMax}");
                    return BadRequest("min must be smaller than max and both within allowed range");
                }

                var n = _Random.Next(min, max);
                _logger.LogDebug($"Instance: {_instance}. Call: {_CallCount}. Returning random number: {n}, from min: {min}, max: {max}");

                var failAfterCallCount = _config.GetValue<int>("Rng:FailAfter:CallCount");
                if (failAfterCallCount > 0 && _CallCount >= failAfterCallCount)
                {
                    Status.Healthy = false;
                }
                return Ok(n);
            }
            else
            {
                var message = $"Instance: {_instance}. Unhealthy!";
                _logger.LogWarning(message);
                return StatusCode(500, new { message = message });
            }
        }
    }
}

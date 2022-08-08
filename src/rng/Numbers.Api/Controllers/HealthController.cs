using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Numbers.Api.Controllers
{
    [ApiController]
    [Route("healthz")]
    public class HealthController : ControllerBase
    {
        private readonly string _instance;

        public HealthController()
        {
            _instance = Dns.GetHostName();
        }

        [HttpGet]
        public IActionResult Get()
        {
            if (Status.Healthy)
            {                
                return Ok($"Instance: {_instance}. Ok");
            }
            else
            {
                return StatusCode(500, new { message = $"Instance: {_instance}. Unhealthy" });
            }
        }
    }
}

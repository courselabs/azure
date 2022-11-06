using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Numbers.Api.Function
{
    public static class rng
    {
        private static Random _Random = new Random();

        [FunctionName("rng")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("rng function called");

            var min = 1;
            var max = 1000;          
            try 
            {
                min = int.Parse(req.Query["min"]);
                max = int.Parse(req.Query["max"]);
            }
            catch (Exception ex)
            {
                log.LogWarning("No valid min & max in querystring - using defaults");
            }

            var n = _Random.Next(min, max);             
            log.LogDebug($"Returning random number: {n}, from min: {min}, max: {max}");
            return new OkObjectResult(n);
        }
    }
}
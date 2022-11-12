using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CalendarProj
{
    public static class HttpDay
    {
        [FunctionName("HttpDay")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req, ILogger log)
        {
            log.LogInformation("HTTP Day trigger activated");
            return new OkObjectResult(DateTime.UtcNow.DayOfWeek);
        }
    }
}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace _2FA
{
    public static class Authenticate
    {
        [FunctionName("Authenticate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableClient starter,
            ILogger log)
        {
            log.LogInformation("Authenticate HTTP triggered");

            var number = req.Query["number"].ToString();
            if (!number.StartsWith("+"))
            {
                number = $"+{number}";
            }
            var instanceId = await starter.StartNewAsync<string>("SmsVerify", number);

            log.LogInformation($"Started SmsVerify with number: {number}; instance: {instanceId}");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}

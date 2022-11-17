using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace ToDoList.Functions;

public static class Negotiate
{
    [FunctionName("negotiate")]
    public static SignalRConnectionInfo Run(
        [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
        [SignalRConnectionInfo(HubName = "serverless", ConnectionStringSetting = "SignalRConnectionString")] SignalRConnectionInfo connectionInfo)
    {
        return connectionInfo;
    }
}
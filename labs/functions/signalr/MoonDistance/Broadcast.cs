using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace MoonDistance;

public static class Broadcast
{
    private static Random _Random = new Random();

    [FunctionName("broadcast")]
    public static async Task Run(
        [TimerTrigger("*/10 * * * * *")] TimerInfo myTimer,
        [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages)
    {
        // https://spaceplace.nasa.gov/moon-distance/en/
        var distance = _Random.Next(225623, 252088);
        await signalRMessages.AddAsync(
            new SignalRMessage
            {
                Target = "newMessage",
                Arguments = new[] { $"The moon is currently: {distance} miles away" }
            });
    }
}
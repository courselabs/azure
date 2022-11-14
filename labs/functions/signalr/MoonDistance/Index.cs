using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace MoonDistance;

public static class Index
{
    [FunctionName("index")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, ExecutionContext context)
    {
        var path = Path.Combine(context.FunctionAppDirectory, "content", "index.html");
        return new ContentResult
        {
            Content = File.ReadAllText(path),
            ContentType = "text/html",
        };
    }
}
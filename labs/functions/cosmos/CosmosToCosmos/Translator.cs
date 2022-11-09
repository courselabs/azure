using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CosmosToCosmos
{
    public static class Translator
    {
        [FunctionName("Translator")]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: "%DatabaseName%",
                collectionName: "posts",
                ConnectionStringSetting = "CosmosDbConnectionString",
                CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [CosmosDB(
                databaseName: "%DatabaseName%",
                collectionName: "posts",
                ConnectionStringSetting = "CosmosDbConnectionString")] IAsyncCollector<dynamic> output,
            ILogger log)
        {            
            if (input != null && input.Count > 0)
            {
                log.LogInformation($"Processing: {input.Count} documents");
                foreach (var document in input)
                {
                    var lang = document.GetPropertyValue<string>("lang");
                    var message = document.GetPropertyValue<string>("message");
                    if (lang == "en" && message == "hello")
                    {
                        log.LogInformation($"Translating message for document ID: {document.Id}");   
                        var translated = new 
                        { 
                            id = Guid.NewGuid(),
                            message = "hola",
                            lang = "es"
                        };
                        await output.AddAsync(translated);
                        log.LogInformation($"Added translated document ID: {translated.id}");
                    }
                }                
            }            
        }
    }
}
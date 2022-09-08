// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using Azure.Messaging;

namespace BlobProcessor
{
    public static class BlobProcessor
    {
        [FunctionName("BlobProcessor")]
        public static void Run(
            [EventGridTrigger]CloudEvent e, 
            ILogger log)
        {
            log.LogInformation("Event received {type} {subject}", e.Type, e.Subject);
            //log.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}

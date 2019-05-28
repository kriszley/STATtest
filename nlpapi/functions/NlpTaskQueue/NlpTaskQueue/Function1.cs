using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace NlpTaskQueue
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([QueueTrigger("testqueue", Connection = "AzureWebJobsStorage")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}

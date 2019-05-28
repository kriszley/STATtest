using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NlpTaskQueue
{
    public static class QueueFunction
    {
        [FunctionName("queuefunction")]
        public static async void Run([QueueTrigger("testqueue", Connection = "AzureWebJobsStorage")]string queueItem, TraceWriter log)
        {
            // TODO: Check table storage state against batch state

            List<DocumentTaskEntity> runningTasks = DatabaseConnection.db.ListTasks();
            foreach(DocumentTaskEntity x in runningTasks)
            {
                switch(x.State)
                {
                    case "creating":
                        // Enqueue job
                        Guid guid = Guid.NewGuid();

                        break;
                    default:
                        break;
                }
            }

            // creating -> create batch task, change state to "enqueued"
            // cancelling -> delete batch task, change state to "cancelled"

            // enqueued and task running -> change state to "running"
            // task running -> update completion percentage
            // enqueued or running and task finished -> change state to "complete"
            // enqueued or running and task failed -> change state to "failed"

            log.Info("Queue handler executed");
        }
    }
}

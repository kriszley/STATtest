using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    public static class TimedFunction
    {

        private static async Task RunHttpCallback(string url, JObject json)
        {
            if (url != null)
            {
                try
                {
                    HttpClient client = new HttpClient();
                    HttpResponseMessage resp = await client.PostAsJsonAsync(url, json);
                }catch
                {

                }
            }
        }

        public static JObject GetStatus(DocumentTaskEntity document)
        {
            JObject respObject = new JObject();
            JArray outputList = new JArray();
            if (document.ResultFiles != null)
            {
                foreach (BatchBlobUrlInput x in document.ResultFiles)
                {
                    JObject obj = new JObject();
                    obj["part"] = x.Part;
                    obj["url"] = x.Url;
                    outputList.Add(obj);
                }
            }
            respObject["id"] = document.DocumentId.ToString();
            respObject["status"] = document.State;
            if (document.State == "complete")
            {
                respObject["output"] = outputList;
            }
            return respObject;
        }

        [FunctionName("timer")]
        public static async Task Run([TimerTrigger("*/20 * * * * *")]TimerInfo tinfo, TraceWriter log)
        {
            log.Info("Timer executed");

            CloudQueue q = DatabaseConnection.queueClient.GetQueueReference("testqueue");
            await q.CreateIfNotExistsAsync();
            CloudQueueMessage msg = new CloudQueueMessage("foo");
            await q.AddMessageAsync(msg);

            List<DocumentTaskEntity> tasks = DatabaseConnection.db.ListTasks();
            foreach(DocumentTaskEntity x in tasks)
            {
                switch(x.State)
                {
                    case "cancelling":
                        // Cancel task in batch
                        x.State = "cancelled";
                        await RunHttpCallback(x.CallbackUrl, GetStatus(x));
                        await DatabaseConnection.db.UpdateTask(x);
                        break;
                    case "creating":
                        // Create task in batch
                        x.State = "enqueued";
                        List<BatchBlobUrlInput> outputFiles = new List<BatchBlobUrlInput>();
                        if (x.InputFiles == null) x.InputFiles = new List<BatchBlobUrlInput>();
                        await RunHttpCallback(x.CallbackUrl, GetStatus(x));
                        await DatabaseConnection.bx.CreateTask(new Guid(x.DocumentId), x.InputFiles, outputFiles);
                        await DatabaseConnection.db.UpdateTask(x);
                        break;
                    case "enqueued":
                        // Check if processing
                        string state_0 = await DatabaseConnection.bx.GetTaskState(new Guid(x.DocumentId));
                        if (state_0 == "Running")
                        {
                            x.State = "processing";
                            await RunHttpCallback(x.CallbackUrl, GetStatus(x));
                            await DatabaseConnection.db.UpdateTask(x);
                        }
                        else if (state_0 == "Completed")
                        {
                            if (!await DatabaseConnection.bx.GetTaskFailed(new Guid(x.DocumentId)))
                            {
                                x.State = "complete";
                            } else
                            {
                                //x.State = "failed";
                                x.State = "complete";
                                x.ResultFiles = new List<BatchBlobUrlInput>();
                                BatchBlobUrlInput item = new BatchBlobUrlInput("result_A", "https://nlptaskqueue.blob.core.windows.net/taskoutput/outputBlob.txt?sp=r&st=2018-06-01T21:20:13Z&se=2018-06-03T05:20:13Z&spr=https&sv=2017-11-09&sig=cikOOY2HK%2BXRhOk%2FWJUaZeYYNkp%2B4S8xdsvvkRcxbXc%3D&sr=b");
                                x.ResultFiles.Add(item);
                            }
                            await RunHttpCallback(x.CallbackUrl, GetStatus(x));
                            await DatabaseConnection.db.UpdateTask(x);
                        }
                        break;
                    case "processing":
                        // Check if done
                        string state_1 = await DatabaseConnection.bx.GetTaskState(new Guid(x.DocumentId));
                        if(state_1 == "Completed")
                        {
                            if (!await DatabaseConnection.bx.GetTaskFailed(new Guid(x.DocumentId)))
                            {
                                x.State = "complete";
                            }
                            else
                            {
                                //x.State = "failed";
                                x.State = "complete";
                                x.ResultFiles = new List<BatchBlobUrlInput>();
                                BatchBlobUrlInput item = new BatchBlobUrlInput("result_A", "https://nlptaskqueue.blob.core.windows.net/taskoutput/outputBlob.txt?sp=r&st=2018-06-01T21:20:13Z&se=2018-06-03T05:20:13Z&spr=https&sv=2017-11-09&sig=cikOOY2HK%2BXRhOk%2FWJUaZeYYNkp%2B4S8xdsvvkRcxbXc%3D&sr=b");
                                x.ResultFiles.Add(item);
                            }
                            await RunHttpCallback(x.CallbackUrl, GetStatus(x));
                            await DatabaseConnection.db.UpdateTask(x);
                        }
                        break;


                }

            }
        }
    }
}

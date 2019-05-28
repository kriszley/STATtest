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
    public static class StatusFunction
    {
        [FunctionName("status")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status/{guidStr}")]HttpRequestMessage req, TraceWriter log, string guidStr)
        {
            Guid guid;
            try
            {
                guid = new Guid(guidStr);
            }
            catch
            {
                return HttpCommon.GenerateJsonError(HttpStatusCode.BadRequest, "invalid_guid");
            }

            JObject responseJson = new JObject();
            DocumentTaskEntity document;
            try
            {
                document = await DatabaseConnection.db.GetTask(guid);
            }
            catch (InvalidOperationException e)
            {
               return HttpCommon.GenerateJsonError(HttpStatusCode.NotFound, guid, "not_found");
            }
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
            responseJson["id"] = guid.ToString();
            responseJson["status"] = document.State;
            if (document.State == "complete")
            {
                responseJson["output"] = outputList;
            }
            return HttpCommon.GenerateJsonResponse(HttpStatusCode.OK, responseJson);


        }
    }
}

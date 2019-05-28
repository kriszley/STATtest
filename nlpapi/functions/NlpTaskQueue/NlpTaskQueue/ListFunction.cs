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
    public static class ListFunction
    {
        [FunctionName("list")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "list/{groupStr}")]HttpRequestMessage req, TraceWriter log, string groupStr)
        {
            List<DocumentTaskEntity> documents;
            try
            {
                documents = await DatabaseConnection.db.ListGroup(groupStr);
            } catch
            {
                JObject errResp = new JObject();
                errResp["group"] = groupStr;
                errResp["error"] = "internal_server_error";
                return HttpCommon.GenerateJsonResponse(HttpStatusCode.InternalServerError, errResp);
            }

            JArray arr = new JArray();
            foreach(DocumentTaskEntity x in documents)
            {
                JObject documentJson = new JObject();
                documentJson["id"] = x.DocumentId.ToString();
                documentJson["status"] = x.State;
                arr.Add(documentJson);
            }

            JObject responseJson = new JObject();
            responseJson["group"] = groupStr;
            responseJson["tasks"] = arr;
            return HttpCommon.GenerateJsonResponse(HttpStatusCode.OK, responseJson);


        }
    }
}

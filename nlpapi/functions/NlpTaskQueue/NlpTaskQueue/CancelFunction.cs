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
    public static class CancelFunction
    {
        [FunctionName("cancel")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cancel/{guidStr}")]HttpRequestMessage req, TraceWriter log, string guidStr)
        {
            Guid guid;
            try
            {
                guid = new Guid(guidStr);
            } catch
            {
                return HttpCommon.GenerateJsonError(HttpStatusCode.BadRequest, "invalid_guid");
            }

            JObject responseJson = new JObject();
            try
            {
                await DatabaseConnection.db.CancelDocument(guid);
            } catch (InvalidOperationException e)
            {
                switch (e.Message) {
                    case "not_found":
                        return HttpCommon.GenerateJsonError(HttpStatusCode.NotFound, guid, "not_found");
                    case "update_failed":
                        return HttpCommon.GenerateJsonError(HttpStatusCode.Gone, guid, "update_failed");
                    case "wrong_state":
                        return HttpCommon.GenerateJsonError(HttpStatusCode.BadRequest, guid, "wrong_state"); ;
                    default:
                        return HttpCommon.GenerateJsonError(HttpStatusCode.InternalServerError, guid, "internal_server_error");

                }
            }
            responseJson["id"] = guid.ToString();
            responseJson["status"] = "cancelling";
            return HttpCommon.GenerateJsonResponse(HttpStatusCode.OK, responseJson);


        }
    }
}

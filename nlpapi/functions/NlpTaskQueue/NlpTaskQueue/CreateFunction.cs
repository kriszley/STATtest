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
    public static class CreateFunction
    {

        [FunctionName("create")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "create")]HttpRequestMessage req, TraceWriter log)
        {
            JObject input;
            string group = null;
            string callback = null;
            List<UrlUploadFile> urlUploadFiles = new List<UrlUploadFile>();
            List<UploadUploadFile> uploadFiles = new List<UploadUploadFile>();
            List<InlineUploadFile> inlineFiles = new List<InlineUploadFile>();

            // Read request
            try
            {
                input = await HttpCommon.ReadRequestJsonObject(req);
                if (input["group"] != null)
                {
                    group = (string)input["group"];
                }
                if (input["callback"] != null)
                {
                    callback = (string)input["callback"];
                }
                JArray inputFilesJArray = (JArray)input["document"];
                foreach (JToken x in inputFilesJArray)
                {
                    JObject inFileObj = (JObject)x;
                    switch ((string)x["type"])
                    {
                        case "inline":
                            {
                                // TODO: Add to list of files to upload immediately
                                string body = (string)x["body"];
                                string part = (string)x["part"];
                                InlineUploadFile inline = new InlineUploadFile(part, body);
                                inlineFiles.Add(inline);
                            }
                            break;
                        case "upload":
                            {
                                // TODO: Get URL / SAS for provided GUID
                                string id = (string)x["id"];
                                Guid uid = new Guid(id);
                                string part = (string)x["part"];
                                UploadUploadFile upload = new UploadUploadFile(part, uid);
                                uploadFiles.Add(upload);
                            }
                            break;
                        case "url":
                            {
                                // TODO: Check if url is in blob storage
                                string url = (string)x["url"];
                                string part = (string)x["part"];
                                UrlUploadFile urlFile = new UrlUploadFile(part, url);
                                urlUploadFiles.Add(urlFile);
                            }
                            break;
                        default:
                            return HttpCommon.GenerateJsonError(HttpStatusCode.BadRequest, "bad_request");
                            //break;
                    }

                }

            }
            catch
            {
                return HttpCommon.GenerateJsonError(HttpStatusCode.BadRequest, "bad_request");
            }

            List<BatchBlobUrlInput> inputFileSasUrls = new List<BatchBlobUrlInput>();

            // Upload inline files
            foreach (InlineUploadFile x in inlineFiles)
            {
                Guid inlineGuid = await DatabaseConnection.blx.UploadInlineFile(x.Body);
                string inlineFileSasUrl = DatabaseConnection.blx.GetFileSasReadUrl(inlineGuid);
                BatchBlobUrlInput inlineUrlInput = new BatchBlobUrlInput(x.Part, inlineFileSasUrl);
                inputFileSasUrls.Add(inlineUrlInput);
            }

            // Also need to create specification file with list of input files

            // Assume for now that all URLs already point to blob storage
            foreach (UrlUploadFile x in urlUploadFiles)
            {
                BatchBlobUrlInput inx = new BatchBlobUrlInput(x);
                inputFileSasUrls.Add(inx);
            }
            urlUploadFiles = new List<UrlUploadFile>();

            // List of URLs from a combination of inline, file, and blob storage uploads
            // Non-blob-storage URLs (urlUploadFiles) will have to be added later

            // Perform operation and generate response
            try
            {
                JObject responseJson = new JObject();
                Guid guid = await DatabaseConnection.db.AddNewDocument(group, callback, urlUploadFiles, inputFileSasUrls);
                responseJson["id"] = guid.ToString();
                responseJson["status"] = "creating";
                return HttpCommon.GenerateJsonResponse(HttpStatusCode.OK, responseJson);
            }
            catch (InvalidOperationException e)
            {
                return HttpCommon.GenerateJsonError(HttpStatusCode.InternalServerError, "internal_server_error");
            }

        }
    }
}

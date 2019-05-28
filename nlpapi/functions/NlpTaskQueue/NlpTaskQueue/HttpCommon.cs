using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NlpTaskQueue
{
    static class HttpCommon
    {
        public static HttpResponseMessage GenerateJsonError(HttpStatusCode status, string statusText)
        {
            HttpResponseMessage resp = new HttpResponseMessage(status);
            JObject errJson = new JObject();
            errJson["error"] = statusText;
            return GenerateJsonResponse(status, errJson);
        }

        public static HttpResponseMessage GenerateJsonError(HttpStatusCode status, Guid guid, string statusText)
        {
            HttpResponseMessage resp = new HttpResponseMessage(status);
            JObject errJson = new JObject();
            errJson["id"] = guid.ToString();
            errJson["error"] = statusText;
            return GenerateJsonResponse(status, errJson);
        }

        public static HttpResponseMessage GenerateJsonResponse(HttpStatusCode status, JToken responseData)
        {
            HttpResponseMessage resp = new HttpResponseMessage(status);
            resp.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseData)));
            resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            resp.Headers.Add("Arr-Disable-Session-Affinity", "True");
            return resp;
        }

        public static async Task<JObject> ReadRequestJsonObject(HttpRequestMessage req)
        {
            byte[] reqData = await req.Content.ReadAsByteArrayAsync();
            JObject input = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(reqData));
            return input;
        }
    }
}

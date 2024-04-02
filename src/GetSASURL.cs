using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Web.Http;

namespace docvault_linkfunc
{
    public static class GetSASURL
    {

        private static readonly AzureStorageHandler StorageHandler = new AzureStorageHandler();

        [FunctionName("GetSASURL")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetSASURL")] HttpRequest req, ILogger log)
        {
            URLRequest? urlRequest = await GetRequest(req);

            if (urlRequest is null)
            {
                return new BadRequestObjectResult((Object?)null);
            }
            string? url = null;
            try
            {
                url = await StorageHandler.GetURL(urlRequest.FileName);
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new InternalServerErrorResult();
            }
            if (url is null)
            {
                return new NotFoundObjectResult((Object?)null);
            }
            return new OkObjectResult(new URLReply(urlRequest.FileName, url));
        }

        private static async Task<URLRequest?> GetRequest(HttpRequest req)
        {
            string json = await new StreamReader(req.Body).ReadToEndAsync();
            if (String.IsNullOrEmpty(json))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<URLRequest>(json);
        }
    }
}

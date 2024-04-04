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
    public static class PurgeSASURL
    {

        private static readonly AzureStorageHandler StorageHandler = AzureStorageHandler.INSTANCE;
        private static readonly AzureCosmosHandler CosmosHandler = AzureCosmosHandler.INSTANCE;

        [FunctionName("PurgeSASURL")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "PurgeSASURL")] HttpRequest req, ILogger log)
        {
            try
            {
                URLRequest? urlRequest = await GetRequest(req);
                if (urlRequest is null)
                {
                    return new BadRequestObjectResult((Object?)null);
                }

                URLReply? url = await CosmosHandler.GetFile(urlRequest.FileName);
                if (url is not null)
                {
                    await CosmosHandler.DeleteFile(url);
                    return new OkObjectResult((Object?)null);
                }
                else
                {
                    return new NotFoundObjectResult((Object?)null);
                }
            }
            catch (Exception e)
            {
                if (e.GetType().Equals(typeof(Microsoft.Azure.Cosmos.CosmosException)))
                {
                    if (((Microsoft.Azure.Cosmos.CosmosException)e).StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        log.LogError("Cannot contact CosmosDB.");
                        return new InternalServerErrorResult();
                    }
                }
                log.LogError(e.ToString());
                return new InternalServerErrorResult();
            }
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

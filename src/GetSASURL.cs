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
        private static readonly AzureCosmosHandler CosmosHandler = new AzureCosmosHandler();

        [FunctionName("GetSASURL")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetSASURL")] HttpRequest req, ILogger log)
        {
            try
            {
                URLRequest? urlRequest = await GetRequest(req);

                if (urlRequest is null)
                {
                    return new BadRequestObjectResult((Object?)null);
                }
                URLReply? url = null;

                URLReply? potential = await CosmosHandler.GetFile(urlRequest.FileName);
                if (potential is not null)
                {
                    if (!(DateTimeOffset.UtcNow.CompareTo(potential.expires) < 0))
                    {
                        log.LogInformation("Cached response is expired.");
                        await CosmosHandler.DeleteFile(potential);
                    }
                    else
                    {
                        log.LogInformation("Used cached response.");
                        return new OkObjectResult(potential);
                    }
                }
                else
                {
                    log.LogInformation("Potential is null.");
                }
                log.LogInformation("Generating URL...");
                url = await StorageHandler.GetURL(urlRequest.FileName);

                if (url is null)
                {
                    return new NotFoundObjectResult((Object?)null);
                }
                await CosmosHandler.SaveFile(url);
                return new OkObjectResult(url);
            }
            catch (Exception e)
            {
                if (e.GetType().Equals(typeof(Microsoft.Azure.Cosmos.CosmosException)))
                {
                    if (((Microsoft.Azure.Cosmos.CosmosException)e).StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                    {
                        log.LogError("Cannot contact CosmosDB.");
                    }
                }
                else
                {
                    log.LogError(e.ToString());
                }
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

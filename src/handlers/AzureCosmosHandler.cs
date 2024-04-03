using Azure.Identity;
using Microsoft.Azure.Cosmos;

public class AzureCosmosHandler
{

    private CosmosClient Client;
    private Database Database;
    private Container Container;

    public AzureCosmosHandler()
    {
        Client = new CosmosClient("https://shonx-document-vault.documents.azure.com/", new DefaultAzureCredential());
        Database = Client.GetDatabase("documents");
        Container = Database.GetContainer("urls");
    }

    public async Task<URLReply?> GetFile(string FileName)
    {
        string sql = "SELECT * FROM documents c WHERE c.id = @id";
        var query = new QueryDefinition(sql).WithParameter("@id", FileName);
        using (FeedIterator<CosmosURL> feed = Container.GetItemQueryIterator<CosmosURL>(query))
        {
            FeedResponse<CosmosURL> response = await feed.ReadNextAsync();
            if(response.Count <= 0)
                return null;
            if (response.Count == 1)
                foreach (CosmosURL url in response)
                    return new URLReply(url);
            if(response.Count > 1)
                throw new InvalidOperationException($"{response.Count} files with name {FileName} returned.");
            return null;
        }
    }

    public Task<ItemResponse<CosmosURL>> SaveFile(URLReply url)
    {
        CosmosURL cosmosURL = new CosmosURL(url);
        return Container.CreateItemAsync(cosmosURL, new PartitionKey(cosmosURL.id));
    }

    public Task<ItemResponse<CosmosURL>> DeleteFile(URLReply url)
    {
        CosmosURL cosmosURL = new CosmosURL(url);
        return Container.DeleteItemAsync<CosmosURL>(cosmosURL.id, new PartitionKey(cosmosURL.id));
    }

}
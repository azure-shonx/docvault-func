using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

public class AzureStorageHandler
{

    private BlobServiceClient bsc;
    private BlobContainerClient bcc;

    public AzureStorageHandler()
    {
        bsc = new BlobServiceClient(
                new Uri("https://shonxdocvault.blob.core.windows.net"),
                new DefaultAzureCredential());
        bcc = bsc.GetBlobContainerClient("documents");
    }

    public async Task<URLReply?> GetURL(string FileName)
    {
        BlobClient bc = bcc.GetBlobClient(FileName);
        if (!await bc.ExistsAsync())
        {
            return null;
        }
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset expires = DateTimeOffset.UtcNow.AddHours(6);

        UserDelegationKey userDelegationKey = await bsc.GetUserDelegationKeyAsync(now, expires);

        BlobSasBuilder sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = bc.BlobContainerName,
            BlobName = bc.Name,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(6)
        };

        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        BlobUriBuilder uriBuilder = new BlobUriBuilder(bc.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(
                userDelegationKey,
                bsc.AccountName
            )
        };
        Uri? uri = uriBuilder.ToUri();
        if (uri is null)
            return null;
        return new URLReply(FileName, uri.ToString(), expires);
    }
}
using System.Text;
using Azure.Storage.Blobs;
using NTK24.Interfaces;

namespace NTK24.Init.Services;

public class StorageScriptDownloader(string scriptContainerName, string connectionString) : IScriptDownloader
{
    public async Task<string> GetScriptAsync(string name)
    {
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(scriptContainerName);
        var blobClient = containerClient.GetBlobClient(name);
        if (!await blobClient.ExistsAsync())
            return string.Empty;
        var downloadedContent = await blobClient.DownloadContentAsync();
        var downloadedProfile = Encoding.UTF8.GetString(downloadedContent.Value.Content);
        return string.IsNullOrEmpty(downloadedProfile) ? string.Empty : downloadedProfile;
    }
}
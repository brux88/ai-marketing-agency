using AiMarketingAgency.Application.Common.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AiMarketingAgency.Infrastructure.FileStorage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobContainerClient? _containerClient;
    private readonly string _localBasePath;
    private readonly string? _publicBaseUrl;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly bool _useAzure;

    public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        _publicBaseUrl = configuration["PublicBaseUrl"]?.TrimEnd('/');

        var connectionString = configuration["AzureStorage:ConnectionString"];
        var containerName = configuration["AzureStorage:ContainerName"] ?? "uploads";

        _localBasePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            _useAzure = true;
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            _logger.LogInformation("AzureBlobStorageService configured with Azure Blob Storage, container: {Container}", containerName);
        }
        else
        {
            _useAzure = false;
            _logger.LogInformation("AzureBlobStorageService falling back to local file storage (wwwroot)");
        }
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default)
    {
        if (_useAzure && _containerClient != null)
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

            var blobClient = _containerClient.GetBlobClient(fileName);
            var headers = new BlobHttpHeaders { ContentType = contentType };

            await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers }, ct);

            var url = blobClient.Uri.ToString();
            _logger.LogInformation("Uploaded blob: {Url}", url);
            return url;
        }

        // Local fallback
        var subFolder = GetSubFolderFromFileName(fileName);
        var dir = Path.Combine(_localBasePath, subFolder);
        Directory.CreateDirectory(dir);
        var fullPath = Path.Combine(dir, fileName);

        await using (var fileStream = File.Create(fullPath))
        {
            await stream.CopyToAsync(fileStream, ct);
        }

        var publicUrl = !string.IsNullOrWhiteSpace(_publicBaseUrl)
            ? $"{_publicBaseUrl}/{subFolder}/{fileName}"
            : $"/{subFolder}/{fileName}";

        _logger.LogInformation("Saved file locally: {Path}", fullPath);
        return publicUrl;
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return;

        if (_useAzure && _containerClient != null)
        {
            // Extract blob name from URL
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var blobName = uri.Segments.Length > 2
                    ? string.Join("", uri.Segments.Skip(2)) // skip "/" and "container/"
                    : Path.GetFileName(uri.LocalPath);
                await _containerClient.DeleteBlobIfExistsAsync(blobName, cancellationToken: ct);
                _logger.LogInformation("Deleted blob: {BlobName}", blobName);
            }
        }
        else
        {
            // Local fallback
            var relativePath = url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_localBasePath, relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted local file: {Path}", fullPath);
            }
        }
    }

    private static string GetSubFolderFromFileName(string fileName)
    {
        if (fileName.StartsWith("proj_") || fileName.StartsWith("agency_"))
            return "logos";
        return "generated-images";
    }
}

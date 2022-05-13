using blobstorage.Services;
using Microsoft.AspNetCore.Mvc;

namespace blobstorage.Controllers;

[ApiController]
[Route("[controller]")]
public class BlobStorageController : ControllerBase
{
    private readonly IAzureBlobStorageService _azureBlobStorageService;

    public BlobStorageController(IAzureBlobStorageService azureBlobStorageService)
    {
        _azureBlobStorageService = azureBlobStorageService;
    }

    [HttpPost(Name = "upload")]
    public async Task<IActionResult> UploadFileAsync([FromForm] UploadFile request,
        CancellationToken cancellationToken)
    {
        var fileName = $"{Guid.NewGuid():N}";
        var blobName = $"{fileName}{Path.GetExtension(request.File.FileName)}";
        var blobUrl = await _azureBlobStorageService.SaveBlobAsync(request.File,
            blobName, request.ContainerName, cancellationToken);

        return Ok(blobUrl);
    }
}

public class UploadFile
{
    public IFormFile File { get; set; } = null!;
    public string ContainerName { get; set; } = "default-container";
}

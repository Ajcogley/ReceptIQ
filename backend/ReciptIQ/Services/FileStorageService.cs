using ReciptIQ.Services.Interfaces;

namespace ReciptIQ.API.Services.Implementations;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string folder = "receipts")
    {
        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", folder);

        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Path.Combine("uploads", folder, fileName);
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_environment.WebRootPath, filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string GetFileUrl(string filePath)
    {
        return $"/{filePath.Replace("\\", "/")}";
    }
}

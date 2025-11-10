namespace ReciptIQ.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string folder = "receipts");
    Task<bool> DeleteFileAsync(string filePath);
    string GetFileUrl(string filePath);
}

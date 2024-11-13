namespace FileShare.Interfaces;

public interface IFileStorageService
{
    Task SaveFileAsync(IFormFile file, string fileId);
    Task<byte[]> ReadFileAsBytesAsync(string fileId);
    void DeleteFile(string fileId);
}
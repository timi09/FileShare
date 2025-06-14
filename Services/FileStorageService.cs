using FileShare.Interfaces;
using FileShare.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace FileShare.Services;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _fileStorageSettings;
    private readonly ISecretKeyProvider _secretKeyProvider;

    public FileStorageService(IOptions<FileStorageSettings> fileStorageSettings, ISecretKeyProvider secretKeyProvider)
    {
        _fileStorageSettings = fileStorageSettings.Value;
        _secretKeyProvider = secretKeyProvider;
    }

    public async Task SaveFileAsync(IFormFile file, string fileId)
    {
        var filePath = Path.Combine(_fileStorageSettings.Path, fileId);

        using (var aes = Aes.Create())
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            aes.IV = new byte[16];
            aes.Key = _secretKeyProvider.GetSecretKey();
            fileStream.Write(aes.IV);
            using (var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write, true))
            {
                await file.CopyToAsync(cryptoStream);
            }
        }
    }

    public async Task<byte[]> ReadFileAsBytesAsync(string fileId)
    {
        var filePath = Path.Combine(_fileStorageSettings.Path, fileId);
        
        using (Aes aes = Aes.Create())
        using (var fileStream = new FileStream(filePath, FileMode.Open))
        {
            aes.IV = new byte[16];
            aes.Key = _secretKeyProvider.GetSecretKey();
            fileStream.Read(aes.IV);
            using (var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read, true))
            {
                using (var resultStream = new MemoryStream())
                {
                    await cryptoStream.CopyToAsync(resultStream);
                    return resultStream.ToArray();
                }
            }
        }
    }

    public void DeleteFile(string fileId)
    {
        var filePath = Path.Combine(_fileStorageSettings.Path, fileId);
        File.Delete(filePath);
    }
}

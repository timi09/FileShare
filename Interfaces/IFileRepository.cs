using FileShare.Models;

namespace FileShare.Interfaces;

public interface IFileRepository
{
    public void SaveFile(FileModel file);
    public void DeleteFile(int id);
    public FileModel? GetFile(int id);
    public List<FileModel> GetAllFiles();
}
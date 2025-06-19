using FileShare.Models;

namespace FileShare.Interfaces;

public interface ILinkGenerateService
{
    public LinkModel GenerateLink(FileModel file, int length);
}

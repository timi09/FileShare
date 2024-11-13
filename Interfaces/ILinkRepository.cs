using FileShare.Models;

namespace FileShare.Interfaces;

public interface ILinkRepository
{
    public void SaveLink(LinkModel link);
    public LinkModel? GetLink(string id);
    public LinkModel? GetLinkByFile(int fileId);
    public void DeleteLink(string id);
}

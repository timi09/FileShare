namespace FileShare.Models;

public class LinkModel
{
    public string Id { get; set; }
    public string FileId { get; set; }
    public FileModel File { get; set; }
    public int CurrentDownloadCount { get; set; }
    public int MaxDownloadCount { get; set; } = 1;
}

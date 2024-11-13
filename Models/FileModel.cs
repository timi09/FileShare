namespace FileShare.Models;

public class FileModel
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public DateTime UploadTime { get; set; }
    public long SizeInBytes { get; set; }
}

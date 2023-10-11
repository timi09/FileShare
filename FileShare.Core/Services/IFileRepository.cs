namespace FileShare.Services
{
    public interface IFileRepository
    {
        public void SaveFile(Entities.File file);
        public void DeleteFile(int id);
        public Entities.File? GetFile(int id);
        public List<Entities.File> GetAllFiles();
    }
}

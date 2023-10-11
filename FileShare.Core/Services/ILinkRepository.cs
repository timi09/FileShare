namespace FileShare.Services
{
    public interface ILinkRepository
    {
        public void SaveLink(Entities.Link link);
        public Entities.Link? GetLink(string id);
        public Entities.Link? GetLinkByFile(int fileId);
        public void DeleteLink(string id);
    }
}

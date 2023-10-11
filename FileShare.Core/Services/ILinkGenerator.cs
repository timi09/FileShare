namespace FileShare.Core.Services
{
    public interface ILinkGenerator
    {
        public Entities.Link GenerateLink(Entities.File file);
    }
}

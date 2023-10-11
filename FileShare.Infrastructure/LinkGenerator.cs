using FileShare.Core.Services;
using FileShare.Entities;

namespace FileShare.Infrastructure
{
    public class LinkGenerator : ILinkGenerator
    {
        private const int LinkLen = 5;
        private static readonly Random random = new Random();
        private static readonly char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

        public static string RandomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public Link GenerateLink(Entities.File file)
        {
            return new Link() { Id = RandomString(LinkLen), File = file };
        }
    }
}

using FileShare.Constants;
using FileShare.Interfaces;
using FileShare.Models;

namespace FileShare.Services;

public class RandomLinkGenerateService : ILinkGenerateService
{
    private static readonly Random random = new Random();
    
    public LinkModel GenerateLink(FileModel file, int length)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(length, LinkConstants.MinLength);

        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, LinkConstants.MaxLength);

        return new LinkModel() { Id = RandomString(length), File = file };
    }

    private string RandomString(int length)
    {
        return new string(Enumerable.Repeat(LinkConstants.AllowedChars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

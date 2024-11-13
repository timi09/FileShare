using FileShare.Constants;

namespace FileShare.Helpers;

public static class FileSizeHelper
{
    public static string GetFileSizeString(long sizeInBytes)
    {
        string postfix = "bytes";
        double value = sizeInBytes;

        if (sizeInBytes > FileSizeConstants.BytesInMb)
        {
            postfix = "Mb";
            value = GetFileSizeInMb(sizeInBytes);
        }
        else if (sizeInBytes > FileSizeConstants.BytesInKb)
        {
            postfix = "Kb";
            value = GetFileSizeInKb(sizeInBytes);
        }

        return $"{value} {postfix}";
    }

    public static double GetFileSizeInMb(long sizeInBytes)
    {
        return Math.Round((double)sizeInBytes / FileSizeConstants.BytesInMb, 2);
    }

    public static double GetFileSizeInKb(long sizeInBytes)
    {
        return Math.Round((double)sizeInBytes / FileSizeConstants.BytesInMb, 2);
    }
}

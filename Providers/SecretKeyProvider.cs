using FileShare.Interfaces;

namespace FileShare.Providers;

public class SecretKeyProvider : ISecretKeyProvider
{
    private readonly byte[] _secretKey;

    public SecretKeyProvider(byte[] secretKey)
    {
        _secretKey = secretKey;
    }

    public byte[] GetSecretKey()
    {
        return _secretKey;
    }
}

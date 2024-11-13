namespace FileShare.Interfaces;

public interface ISecretKeyProvider
{
    byte[] GetSecretKey();
}
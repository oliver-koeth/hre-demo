using System.Security.Cryptography;

namespace AuthModule.Foundation.Tests.Persistence;

internal static class TestSecrets
{
    public static (string EncryptionKeyPath, string HmacKeyPath) Create(string root)
    {
        Directory.CreateDirectory(root);
        var enc = Path.Combine(root, "enc.key");
        var hmac = Path.Combine(root, "hmac.key");
        File.WriteAllBytes(enc, RandomNumberGenerator.GetBytes(32));
        File.WriteAllBytes(hmac, RandomNumberGenerator.GetBytes(32));
        return (enc, hmac);
    }
}


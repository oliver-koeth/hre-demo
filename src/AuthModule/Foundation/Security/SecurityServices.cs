using System.Security.Cryptography;

namespace AuthModule.Foundation.Security;

public interface IKeyProvider
{
    ReadOnlyMemory<byte> GetEncryptionKey();
    ReadOnlyMemory<byte> GetHmacKey();
}

public interface IEncryptionService
{
    byte[] Encrypt(ReadOnlySpan<byte> plaintext);
    byte[] Decrypt(ReadOnlySpan<byte> encryptedPayload);
}

public interface IIntegrityService
{
    byte[] ComputeSignature(ReadOnlySpan<byte> content);
    bool Verify(ReadOnlySpan<byte> content, ReadOnlySpan<byte> signature);
}

public sealed class KeyProvider : IKeyProvider
{
    private readonly byte[] _encryptionKey;
    private readonly byte[] _hmacKey;

    public KeyProvider(string encryptionKeyPath, string hmacKeyPath)
    {
        _encryptionKey = LoadKey(encryptionKeyPath, expectedLength: 32);
        _hmacKey = LoadKey(hmacKeyPath, expectedLength: 32);
    }

    public ReadOnlyMemory<byte> GetEncryptionKey() => _encryptionKey;
    public ReadOnlyMemory<byte> GetHmacKey() => _hmacKey;

    private static byte[] LoadKey(string path, int expectedLength)
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"Secret key file not found at '{path}'.");
        }

        var raw = File.ReadAllBytes(path);
        if (raw.Length == expectedLength)
        {
            return raw;
        }

        var asText = File.ReadAllText(path).Trim();
        try
        {
            var decoded = Convert.FromBase64String(asText);
            if (decoded.Length == expectedLength)
            {
                return decoded;
            }
        }
        catch (FormatException)
        {
            // Fall through and emit a clear error below.
        }

        throw new InvalidOperationException(
            $"Invalid key material at '{path}'. Expected {expectedLength} bytes raw or Base64-decoded.");
    }
}

public sealed class EncryptionService(IKeyProvider keyProvider) : IEncryptionService
{
    public byte[] Encrypt(ReadOnlySpan<byte> plaintext)
    {
        var key = keyProvider.GetEncryptionKey().Span;
        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[plaintext.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(key, tagSizeInBytes: 16);
        aes.Encrypt(nonce, plaintext, cipher, tag);

        var output = new byte[nonce.Length + cipher.Length + tag.Length];
        nonce.CopyTo(output.AsSpan(0, nonce.Length));
        cipher.CopyTo(output.AsSpan(nonce.Length, cipher.Length));
        tag.CopyTo(output.AsSpan(nonce.Length + cipher.Length, tag.Length));
        return output;
    }

    public byte[] Decrypt(ReadOnlySpan<byte> encryptedPayload)
    {
        if (encryptedPayload.Length < 12 + 16)
        {
            throw new InvalidOperationException("Encrypted payload is too short.");
        }

        var key = keyProvider.GetEncryptionKey().Span;
        var nonce = encryptedPayload[..12];
        var tag = encryptedPayload[^16..];
        var cipher = encryptedPayload[12..^16];
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(key, tagSizeInBytes: 16);
        aes.Decrypt(nonce, cipher, tag, plain);
        return plain;
    }
}

public sealed class IntegrityService(IKeyProvider keyProvider) : IIntegrityService
{
    public byte[] ComputeSignature(ReadOnlySpan<byte> content)
    {
        using var hmac = new HMACSHA256(keyProvider.GetHmacKey().ToArray());
        return hmac.ComputeHash(content.ToArray());
    }

    public bool Verify(ReadOnlySpan<byte> content, ReadOnlySpan<byte> signature)
    {
        var computed = ComputeSignature(content);
        return CryptographicOperations.FixedTimeEquals(computed, signature);
    }
}


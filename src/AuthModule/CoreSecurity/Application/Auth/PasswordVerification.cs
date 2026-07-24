using System.Security.Cryptography;
using System.Text;
using AuthModule.CoreSecurity.Persistence;
using AuthModule.Foundation.Domain.Entities;
using Konscious.Security.Cryptography;

namespace AuthModule.CoreSecurity.Application.Auth;

public interface IPasswordVerificationService
{
    bool Verify(StoredCredential credential, string password);
    StoredCredential HashForNewCredential(Guid userId, string password);
}

public sealed class PasswordVerificationService : IPasswordVerificationService
{
    public bool Verify(StoredCredential credential, string password)
    {
        if (!credential.IsActive)
        {
            return false;
        }

        if (credential.Algorithm == Foundation.Domain.Entities.HashAlgorithm.BCrypt)
        {
            return BCrypt.Net.BCrypt.Verify(password, credential.Hash);
        }

        return VerifyArgon2id(credential, password);
    }

    public StoredCredential HashForNewCredential(Guid userId, string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var salt = Convert.ToBase64String(saltBytes);
        var hash = ComputeArgon2idHash(password, saltBytes);
        return new StoredCredential(userId, Foundation.Domain.Entities.HashAlgorithm.Argon2id, hash, salt, true);
    }

    private static bool VerifyArgon2id(StoredCredential credential, string password)
    {
        byte[] saltBytes;
        try
        {
            saltBytes = Convert.FromBase64String(credential.Salt);
        }
        catch (FormatException)
        {
            return false;
        }

        var expected = ComputeArgon2idHash(password, saltBytes);
        var left = Encoding.UTF8.GetBytes(expected);
        var right = Encoding.UTF8.GetBytes(credential.Hash);
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }

    private static string ComputeArgon2idHash(string password, byte[] salt)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            Iterations = 4,
            MemorySize = 65_536,
            DegreeOfParallelism = 2,
        };
        return Convert.ToBase64String(argon2.GetBytes(32));
    }
}

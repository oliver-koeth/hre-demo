namespace AuthModule.CoreSecurity.Domain;

public static class Normalization
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static string BuildPermissionKey(string resource, string action) =>
        $"{resource.Trim().ToLowerInvariant()}:{action.Trim().ToLowerInvariant()}";
}

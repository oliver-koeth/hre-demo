using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AuthModule.ServiceHost.Tests.Support;

/// <summary>
/// Spins up the real ServiceHost in-process with an isolated temp config.
/// Implements IAsyncLifetime so xUnit creates/disposes it around each test class.
/// </summary>
public sealed class ServiceHostFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // Serializes host creation so parallel test classes don't race on POLICY_CONFIG_PATH.
    private static readonly SemaphoreSlim _hostInitLock = new(1, 1);

    private readonly TestPolicyConfig _config = new();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _hostInitLock.Wait();
        try
        {
            Environment.SetEnvironmentVariable("POLICY_CONFIG_PATH", _config.ConfigPath);
            return base.CreateHost(builder);
        }
        finally
        {
            Environment.SetEnvironmentVariable("POLICY_CONFIG_PATH", null);
            _hostInitLock.Release();
        }
    }

    /// <summary>Creates an HttpClient already configured for this test host.</summary>
    public new HttpClient CreateClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = false,
    });

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        _config.Dispose();
    }
}

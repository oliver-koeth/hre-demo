using System.Threading.Channels;

namespace AuthModule.Foundation.Runtime;

public interface IStoreWriteCoordinator
{
    Task EnqueueAsync(string storeName, Func<CancellationToken, Task> workItem, CancellationToken cancellationToken = default);
}

public sealed class StoreWriteCoordinator : IStoreWriteCoordinator, IAsyncDisposable
{
    private readonly Dictionary<string, Channel<Func<CancellationToken, Task>>> _channels = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Task> _workers = [];
    private readonly CancellationTokenSource _cts = new();

    public Task EnqueueAsync(string storeName, Func<CancellationToken, Task> workItem, CancellationToken cancellationToken = default)
    {
        if (!_channels.TryGetValue(storeName, out var channel))
        {
            channel = Channel.CreateUnbounded<Func<CancellationToken, Task>>();
            _channels[storeName] = channel;
            _workers.Add(Task.Run(() => RunWorkerAsync(channel.Reader, _cts.Token), _cts.Token));
        }

        return channel.Writer.WriteAsync(workItem, cancellationToken).AsTask();
    }

    private static async Task RunWorkerAsync(ChannelReader<Func<CancellationToken, Task>> reader, CancellationToken cancellationToken)
    {
        await foreach (var work in reader.ReadAllAsync(cancellationToken))
        {
            await work(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        foreach (var channel in _channels.Values)
        {
            channel.Writer.TryComplete();
        }

        await Task.WhenAll(_workers);
        _cts.Dispose();
    }
}

public static class RetryPolicy
{
    public static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, int maxAttempts = 3, int initialDelayMs = 50)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        var delay = initialDelayMs;
        Exception? last = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await action();
            }
            catch (IOException ex) when (attempt < maxAttempts)
            {
                last = ex;
                await Task.Delay(delay + Random.Shared.Next(0, 20));
                delay *= 2;
            }
        }

        throw last ?? new InvalidOperationException("Retry policy failed with no captured exception.");
    }
}

public interface IBackupRestoreService
{
    Task RestoreLatestSnapshotAsync(string storePath, CancellationToken cancellationToken);
}

public sealed class RecoveryOrchestrator(IBackupRestoreService backupRestoreService)
{
    public async Task QuarantineAndRestoreAsync(string storePath, CancellationToken cancellationToken = default)
    {
        var quarantinePath = $"{storePath}.quarantine.{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";
        if (File.Exists(storePath))
        {
            File.Move(storePath, quarantinePath, overwrite: true);
        }

        if (File.Exists($"{storePath}.sig"))
        {
            File.Move($"{storePath}.sig", $"{quarantinePath}.sig", overwrite: true);
        }

        await backupRestoreService.RestoreLatestSnapshotAsync(storePath, cancellationToken);
    }
}


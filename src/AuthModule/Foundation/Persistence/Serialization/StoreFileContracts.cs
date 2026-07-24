using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuthModule.Foundation.Persistence.Serialization;

public sealed class StoreFileHeader
{
    public string SchemaVersion { get; set; } = StoreSchema.CurrentVersion;
    public string StoreType { get; set; } = string.Empty;
    public int RecordCount { get; set; }
}

public sealed class StoreEnvelope<T>
{
    public StoreFileHeader Header { get; set; } = new();
    public List<T> Records { get; set; } = [];
}

public static class StoreSchema
{
    public const string CurrentVersion = "1.0";
}

public static class JsonStoreSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
    };
}


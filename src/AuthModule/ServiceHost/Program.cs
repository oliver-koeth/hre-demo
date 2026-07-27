using System.Text.Json;
using AuthModule.CoreSecurity.Api;
using AuthModule.CoreSecurity.Bootstrap;
using AuthModule.CoreSecurity.Configuration;
using AuthModule.Foundation.Api;
using AuthModule.Foundation.Bootstrap;
using AuthModule.Foundation.Configuration;
using AuthModule.Governance.Api;
using AuthModule.Governance.Bootstrap;
using AuthModule.Governance.Configuration;
using AuthModule.Integration.Api;
using AuthModule.Integration.Bootstrap;
using AuthModule.Integration.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var configurationPath = ResolvePolicyConfigurationPath(builder.Environment.ContentRootPath);
var runtimeConfiguration = RuntimeConfigurationLoader.Load(configurationPath);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Authentication Module API",
        Version = "v1",
        Description = "OpenAPI documentation for the Auth Module service host covering diagnostics, core security, governance, and integration APIs.",
    });
});

builder.Services.AddFoundationServices(runtimeConfiguration.Policy);
builder.Services.AddCoreSecurityServices(runtimeConfiguration.CoreSecurity);
builder.Services.AddGovernanceServices(runtimeConfiguration.Governance);
builder.Services.AddIntegrationServices(new IntegrationConfiguration
{
    RepositoryRootPath = builder.Environment.ContentRootPath,
    GateDecisionTimeoutSeconds = runtimeConfiguration.Integration.GateDecisionTimeoutSeconds,
    GateEvidenceRetentionDays = runtimeConfiguration.Integration.GateEvidenceRetentionDays,
    PreviewRuntimeApproved = runtimeConfiguration.Integration.PreviewRuntimeApproved,
});

var app = builder.Build();

app.UseSwagger(options => options.RouteTemplate = "openapi/{documentName}.json");

app.MapGet("/", () => Results.Ok(new ServiceStatusResponse("auth-module", "Running", configurationPath)))
    .WithName("GetServiceStatus")
    .WithSummary("Gets the service host status.")
    .WithDescription("Returns the running status and the active policy configuration path for the service host.")
    .WithTags("Service Metadata");
app.MapGet("/docs", () => Results.Content(DocumentationAssets.SwaggerUiHtml, "text/html"))
    .ExcludeFromDescription();
app.MapGet("/docs/index.html", () => Results.Content(DocumentationAssets.SwaggerUiHtml, "text/html"))
    .ExcludeFromDescription();
app.MapFoundationDiagnostics();
app.MapCoreSecurityEndpoints();
app.MapGovernanceEndpoints();
app.MapIntegrationEndpoints();

app.Run();

static string ResolvePolicyConfigurationPath(string contentRootPath)
{
    var configuredPath = Environment.GetEnvironmentVariable("POLICY_CONFIG_PATH");
    if (!string.IsNullOrWhiteSpace(configuredPath))
    {
        var candidatePaths = new List<string>();
        if (Path.IsPathRooted(configuredPath))
        {
            candidatePaths.Add(configuredPath);
        }
        else
        {
            candidatePaths.Add(Path.GetFullPath(configuredPath, Directory.GetCurrentDirectory()));
            candidatePaths.Add(Path.GetFullPath(configuredPath, contentRootPath));

            var probe = new DirectoryInfo(contentRootPath);
            while (probe is not null)
            {
                candidatePaths.Add(Path.GetFullPath(configuredPath, probe.FullName));
                probe = probe.Parent;
            }
        }

        var existingPath = candidatePaths.FirstOrDefault(File.Exists);
        if (existingPath is null)
        {
            throw new InvalidOperationException(
                $"Policy configuration file not found at '{configuredPath}'. Set POLICY_CONFIG_PATH to a valid file.");
        }

        return existingPath;
    }

    var current = new DirectoryInfo(contentRootPath);
    while (current is not null)
    {
        var candidatePath = Path.Combine(current.FullName, "config", "policy.template.json");
        if (File.Exists(candidatePath))
        {
            return candidatePath;
        }

        current = current.Parent;
    }

    throw new InvalidOperationException(
        $"Policy configuration file was not found from content root '{contentRootPath}'. Set POLICY_CONFIG_PATH to a valid file.");
}

static class RuntimeConfigurationLoader
{
    public static RuntimeConfigurations Load(string configurationPath)
    {
        using var stream = File.OpenRead(configurationPath);
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
        };
        var document = JsonSerializer.Deserialize<RuntimePolicyDocument>(stream, options)
            ?? throw new InvalidOperationException($"Failed to deserialize configuration file '{configurationPath}'.");

        if (document.CoreSecurity is null)
        {
            throw new InvalidOperationException("Configuration section 'coreSecurity' is required.");
        }

        if (document.Governance is null)
        {
            throw new InvalidOperationException("Configuration section 'governance' is required.");
        }

        if (document.Integration is null)
        {
            throw new InvalidOperationException("Configuration section 'integration' is required.");
        }

        var policy = new PolicyConfiguration
        {
            TokenLifetimeSeconds = document.TokenLifetimeSeconds,
            AdminTokenLifetimeSeconds = document.AdminTokenLifetimeSeconds,
            TokenIssuer = document.TokenIssuer ?? string.Empty,
            TokenAudience = document.TokenAudience ?? string.Empty,
            MaxLoginAttempts = document.MaxLoginAttempts,
            LockoutDurationSeconds = document.LockoutDurationSeconds,
            StoreBasePath = document.StoreBasePath ?? string.Empty,
            EncryptionKeyPath = document.EncryptionKeyPath ?? string.Empty,
            HmacKeyPath = document.HmacKeyPath ?? string.Empty,
            AuditEventRetentionDays = document.AuditEventRetentionDays,
            UserRecordRetentionDays = document.UserRecordRetentionDays,
            IncidentRecordRetentionDays = document.IncidentRecordRetentionDays,
            SodApprovalRequiredForRoleChanges = document.SodApprovalRequiredForRoleChanges,
        };

        return new RuntimeConfigurations(policy, document.CoreSecurity, document.Governance, document.Integration);
    }
}

sealed record RuntimeConfigurations(
    PolicyConfiguration Policy,
    CoreSecurityConfiguration CoreSecurity,
    GovernanceConfiguration Governance,
    IntegrationConfiguration Integration);

sealed record ServiceStatusResponse(string Service, string Status, string Configuration);

sealed record RuntimePolicyDocument
{
    public int TokenLifetimeSeconds { get; init; }
    public int AdminTokenLifetimeSeconds { get; init; }
    public string? TokenIssuer { get; init; }
    public string? TokenAudience { get; init; }
    public int MaxLoginAttempts { get; init; }
    public int LockoutDurationSeconds { get; init; }
    public string? StoreBasePath { get; init; }
    public string? EncryptionKeyPath { get; init; }
    public string? HmacKeyPath { get; init; }
    public int AuditEventRetentionDays { get; init; }
    public int UserRecordRetentionDays { get; init; }
    public int IncidentRecordRetentionDays { get; init; }
    public bool SodApprovalRequiredForRoleChanges { get; init; }
    public CoreSecurityConfiguration? CoreSecurity { get; init; }
    public GovernanceConfiguration? Governance { get; init; }
    public IntegrationConfiguration? Integration { get; init; }
}

file static class DocumentationAssets
{
    public const string SwaggerUiHtml = """
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Authentication Module API Docs</title>
  <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui.css" />
  <style>
    :root { color-scheme: dark; }
    body {
      margin: 0;
      background: #0b1020;
      color: #f3f6ff;
      font-family: Inter, Segoe UI, Roboto, Helvetica, Arial, sans-serif;
    }
    .swagger-ui,
    .swagger-ui .opblock-tag,
    .swagger-ui .info p,
    .swagger-ui .info li,
    .swagger-ui .info a,
    .swagger-ui .opblock-summary-path,
    .swagger-ui .opblock-summary-description,
    .swagger-ui table thead tr td,
    .swagger-ui table thead tr th,
    .swagger-ui .parameter__name,
    .swagger-ui .response-col_status,
    .swagger-ui .response-col_description,
    .swagger-ui .markdown p,
    .swagger-ui .model-title,
    .swagger-ui .tab li button.tablinks {
      color: #dfe7ff !important;
    }
    .swagger-ui .scheme-container,
    .swagger-ui .opblock .opblock-section-header,
    .swagger-ui .opblock-body pre,
    .swagger-ui .model-box {
      background: #18233f !important;
    }
    .swagger-ui .info .title,
    .swagger-ui .opblock-tag {
      color: #f8faff !important;
    }
    .swagger-ui .opblock-description-wrapper p,
    .swagger-ui .opblock-external-docs-wrapper p,
    .swagger-ui .opblock-title_normal p {
      color: #c7d5ff !important;
    }
    .swagger-ui .opblock {
      border-color: #3b4f82 !important;
    }
  </style>
</head>
<body>
  <div id="swagger-ui"></div>
  <script src="https://cdn.jsdelivr.net/npm/swagger-ui-dist@5/swagger-ui-bundle.js"></script>
  <script>
    window.ui = SwaggerUIBundle({
      url: '/openapi/v1.json',
      dom_id: '#swagger-ui',
      deepLinking: true,
      docExpansion: 'list',
      defaultModelsExpandDepth: 1,
      presets: [SwaggerUIBundle.presets.apis],
      layout: 'BaseLayout'
    });
  </script>
</body>
</html>
""";
}

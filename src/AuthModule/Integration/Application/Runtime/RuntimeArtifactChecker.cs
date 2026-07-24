using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Configuration;
using AuthModule.Integration.Domain;

namespace AuthModule.Integration.Application.Runtime;

public sealed class RuntimeArtifactChecker(
    IntegrationConfiguration configuration) : IRuntimeArtifactChecker
{
    public Task<Result<IReadOnlyList<BlockingFindingRecord>, DomainError>> CheckAsync(RequestContext context)
    {
        var requiredPaths = new[]
        {
            "docker-compose.yml",
            Path.Combine("config", "policy.template.json"),
        };

        var findings = new List<BlockingFindingRecord>();
        foreach (var path in requiredPaths)
        {
            var resolvedPath = Path.GetFullPath(Path.Combine(configuration.RepositoryRootPath, path));
            if (File.Exists(resolvedPath))
            {
                continue;
            }

            findings.Add(new BlockingFindingRecord
            {
                BlockingFindingId = Guid.NewGuid(),
                Category = "RuntimeArtifact",
                Code = "MISSING_ARTIFACT",
                Description = $"Required artifact '{path}' was not found.",
                IsOpen = true,
                CreatedAt = context.Timestamp,
                ResolvedAt = null,
                CorrelationId = context.CorrelationId,
            });
        }

        return Task.FromResult(Result<IReadOnlyList<BlockingFindingRecord>, DomainError>.Success(findings));
    }
}

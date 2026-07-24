using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Integration.Application.Common;
using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Configuration;
using AuthModule.Integration.Domain;

namespace AuthModule.Integration.Application.Conformance;

public sealed class ContractConformanceChecker(
    IntegrationConfiguration configuration) : IContractConformanceChecker
{
    public Task<Result<IReadOnlyList<ContractConformanceFinding>, DomainError>> CheckAsync(
        IReadOnlyList<EndpointRouteDescriptor> endpoints,
        RequestContext context)
    {
        if (endpoints.Count == 0)
        {
            return Task.FromResult(Result<IReadOnlyList<ContractConformanceFinding>, DomainError>.Failure(
                ErrorFactory.Validation("At least one endpoint descriptor is required.", context)));
        }

        var findings = new List<ContractConformanceFinding>();
        foreach (var endpoint in endpoints.OrderBy(x => x.EndpointKey, StringComparer.Ordinal))
        {
            var endpointPath = ResolvePath(endpoint.EndpointFilePath);
            var problemDetailsPath = ResolvePath(endpoint.ProblemDetailsFilePath);
            if (!File.Exists(endpointPath))
            {
                findings.Add(CreateBlockingFinding(endpoint, "Endpoint file not found.", false, false, false));
                continue;
            }

            if (!File.Exists(problemDetailsPath))
            {
                findings.Add(CreateBlockingFinding(endpoint, "ProblemDetails file not found.", false, false, false));
                continue;
            }

            var endpointContent = File.ReadAllText(endpointPath);
            var problemDetailsContent = File.ReadAllText(problemDetailsPath);

            var hasProblemShape = endpointContent.Contains(".ToProblem()", StringComparison.Ordinal) &&
                                  problemDetailsContent.Contains("ProblemDetails", StringComparison.Ordinal);
            var hasErrorCode = problemDetailsContent.Contains("details.Extensions[\"errorCode\"]", StringComparison.Ordinal);
            var hasCorrelationId = problemDetailsContent.Contains("details.Extensions[\"correlationId\"]", StringComparison.Ordinal);

            var isBlocking = !hasProblemShape || !hasErrorCode || !hasCorrelationId;
            findings.Add(new ContractConformanceFinding
            {
                FindingId = Guid.NewGuid(),
                EndpointKey = endpoint.EndpointKey,
                Route = endpoint.Route,
                HasProblemDetailsShape = hasProblemShape,
                HasErrorCodeExtension = hasErrorCode,
                HasCorrelationIdExtension = hasCorrelationId,
                IsBlocking = isBlocking,
                Message = isBlocking
                    ? "Endpoint does not satisfy required RFC7807 + errorCode/correlationId contract."
                    : "Endpoint satisfies RFC7807 + errorCode/correlationId contract.",
            });
        }

        return Task.FromResult(Result<IReadOnlyList<ContractConformanceFinding>, DomainError>.Success(findings));
    }

    private string ResolvePath(string relativeOrAbsolutePath)
    {
        if (Path.IsPathRooted(relativeOrAbsolutePath))
        {
            return relativeOrAbsolutePath;
        }

        return Path.GetFullPath(Path.Combine(configuration.RepositoryRootPath, relativeOrAbsolutePath));
    }

    private static ContractConformanceFinding CreateBlockingFinding(
        EndpointRouteDescriptor endpoint,
        string message,
        bool hasProblemShape,
        bool hasErrorCode,
        bool hasCorrelationId) =>
        new()
        {
            FindingId = Guid.NewGuid(),
            EndpointKey = endpoint.EndpointKey,
            Route = endpoint.Route,
            HasProblemDetailsShape = hasProblemShape,
            HasErrorCodeExtension = hasErrorCode,
            HasCorrelationIdExtension = hasCorrelationId,
            IsBlocking = true,
            Message = message,
        };
}

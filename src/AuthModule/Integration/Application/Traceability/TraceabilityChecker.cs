using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Integration.Application.Common;
using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Configuration;
using AuthModule.Integration.Domain;

namespace AuthModule.Integration.Application.Traceability;

public sealed class TraceabilityChecker(
    IntegrationConfiguration configuration) : ITraceabilityChecker
{
    public Task<Result<IReadOnlyList<TraceabilityCoverageEntry>, DomainError>> CheckAsync(
        IReadOnlyList<StoryTraceabilityInput> storyMappings,
        RequestContext context)
    {
        var byStory = storyMappings.ToDictionary(x => x.StoryId, StringComparer.OrdinalIgnoreCase);
        var output = new List<TraceabilityCoverageEntry>();

        foreach (var requiredStory in IntegrationStories.RequiredStoryIds)
        {
            if (!byStory.TryGetValue(requiredStory, out var mapping))
            {
                output.Add(new TraceabilityCoverageEntry
                {
                    StoryId = requiredStory,
                    UnitId = "UOW-04",
                    FilePaths = [],
                    IsCovered = false,
                    Note = "Missing story mapping.",
                });
                continue;
            }

            var existingPaths = mapping.FilePaths
                .Where(path => File.Exists(ResolvePath(path)))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            output.Add(new TraceabilityCoverageEntry
            {
                StoryId = requiredStory,
                UnitId = mapping.UnitId,
                FilePaths = existingPaths,
                IsCovered = existingPaths.Count > 0,
                Note = existingPaths.Count > 0 ? "Covered." : "Mapped file paths were not found.",
            });
        }

        return Task.FromResult(Result<IReadOnlyList<TraceabilityCoverageEntry>, DomainError>.Success(output));
    }

    private string ResolvePath(string relativeOrAbsolutePath)
    {
        if (Path.IsPathRooted(relativeOrAbsolutePath))
        {
            return relativeOrAbsolutePath;
        }

        return Path.GetFullPath(Path.Combine(configuration.RepositoryRootPath, relativeOrAbsolutePath));
    }
}

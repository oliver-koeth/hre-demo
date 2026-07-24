using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Domain;
using AuthModule.Integration.Tests.Support;
using FsCheck;
using FsCheck.Xunit;

namespace AuthModule.Integration.Tests.PropertyBased;

public sealed class PbtEvidenceReuseTests
{
    [Property]
    public bool GateFingerprint_ShouldRemainStable_ForEquivalentStoryMappingSets(NonEmptyArray<int> orderHint)
    {
        var sut = TestContextFactory.Create();
        var request = TestContextFactory.BuildRequest();
        var hint = orderHint.Get;
        var shuffled = request.Options.StoryMappings
            .Select((x, i) => new { Mapping = x, Key = hint[i % hint.Length] })
            .OrderBy(x => x.Key)
            .Select(x => x.Mapping)
            .ToList();

        var first = sut.GateService.ExecuteAsync(request, sut.RequestContext).GetAwaiter().GetResult();
        var second = sut.GateService.ExecuteAsync(
            new ExecuteGateRequest(new IntegrationGateOptions(request.Options.Endpoints, shuffled, request.Options.UnitReadiness)),
            sut.RequestContext).GetAwaiter().GetResult();

        return first.IsSuccess &&
               second.IsSuccess &&
               first.Value.Decision.Status == GateDecisionStatus.Pass &&
               second.Value.Decision.Status == GateDecisionStatus.Pass &&
               first.Value.Decision.DecisionFingerprint == second.Value.Decision.DecisionFingerprint;
    }
}

using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Domain;
using AuthModule.Integration.Tests.Support;
using FluentAssertions;

namespace AuthModule.Integration.Tests.Gate;

public sealed class IntegrationGateTests
{
    [Fact]
    public async Task GateOutcome_ShouldBeDeterministic_ForUnchangedInputs()
    {
        var sut = TestContextFactory.Create();
        var request = TestContextFactory.BuildRequest();

        var first = await sut.GateService.ExecuteAsync(request, sut.RequestContext);
        var second = await sut.GateService.ExecuteAsync(request, sut.RequestContext);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        first.Value.Decision.Status.Should().Be(second.Value.Decision.Status);
        first.Value.Decision.DecisionFingerprint.Should().Be(second.Value.Decision.DecisionFingerprint);
    }

    [Fact]
    public async Task GateOutcome_ShouldFailFast_OnBlockingConformanceFinding()
    {
        var sut = TestContextFactory.Create(root =>
        {
            File.WriteAllText(Path.Combine(root, "src", "AuthModule", "CoreSecurity", "Api", "CoreSecurityProblemDetails.cs"), "var details = new ProblemDetails();");
            File.Delete(Path.Combine(root, "docker-compose.yml"));
        });
        var request = TestContextFactory.BuildRequest();

        var result = await sut.GateService.ExecuteAsync(request, sut.RequestContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Decision.Status.Should().Be(GateDecisionStatus.Fail);
        result.Value.Decision.BlockingFindings.Should().ContainSingle(x => x.Category == "ContractConformance");
    }

    [Fact]
    public async Task GateOutcome_ShouldFail_WhenRuntimeArtifactMissing()
    {
        var sut = TestContextFactory.Create(root => File.Delete(Path.Combine(root, "docker-compose.yml")));
        var request = TestContextFactory.BuildRequest();

        var result = await sut.GateService.ExecuteAsync(request, sut.RequestContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Decision.Status.Should().Be(GateDecisionStatus.Fail);
        result.Value.Decision.BlockingFindings.Should().Contain(x => x.Category == "RuntimeArtifact");
    }

    [Fact]
    public async Task GateOutcome_ShouldFail_WhenStoryCoverageIsMissing()
    {
        var sut = TestContextFactory.Create();
        var request = TestContextFactory.BuildRequest();
        var filteredMappings = request.Options.StoryMappings.Where(x => x.StoryId != "US-12").ToList();
        request = new ExecuteGateRequest(new IntegrationGateOptions(request.Options.Endpoints, filteredMappings, request.Options.UnitReadiness));

        var result = await sut.GateService.ExecuteAsync(request, sut.RequestContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.Decision.Status.Should().Be(GateDecisionStatus.Fail);
        result.Value.Decision.BlockingFindings.Should().Contain(x => x.Category == "Traceability");
    }
}

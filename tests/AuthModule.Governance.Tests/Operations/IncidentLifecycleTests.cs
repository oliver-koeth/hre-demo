using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Domain;
using AuthModule.Governance.Tests.Support;
using FluentAssertions;

namespace AuthModule.Governance.Tests.Operations;

public sealed class IncidentLifecycleTests
{
    [Fact]
    public async Task IncidentStatus_ShouldFollowValidProgression()
    {
        var sut = TestContextFactory.Create();
        var create = await sut.IncidentService.CreateAsync(
            new CreateIncidentRequest("auth issue", GovernanceIncidentSeverity.High, "auth-api degraded", false),
            sut.RequestContext);

        var investigating = await sut.IncidentService.AdvanceStatusAsync(
            new AdvanceIncidentStatusRequest(create.Value.IncidentId, GovernanceIncidentStatus.Investigating),
            sut.RequestContext);
        var resolved = await sut.IncidentService.AdvanceStatusAsync(
            new AdvanceIncidentStatusRequest(create.Value.IncidentId, GovernanceIncidentStatus.Resolved),
            sut.RequestContext);

        investigating.IsSuccess.Should().BeTrue();
        resolved.IsSuccess.Should().BeTrue();
        resolved.Value.Status.Should().Be(GovernanceIncidentStatus.Resolved);
    }

    [Fact]
    public async Task IncidentStatus_ShouldRejectInvalidTransition()
    {
        var sut = TestContextFactory.Create();
        var create = await sut.IncidentService.CreateAsync(
            new CreateIncidentRequest("auth issue", GovernanceIncidentSeverity.High, "auth-api degraded", false),
            sut.RequestContext);

        var invalid = await sut.IncidentService.AdvanceStatusAsync(
            new AdvanceIncidentStatusRequest(create.Value.IncidentId, GovernanceIncidentStatus.Closed),
            sut.RequestContext);

        invalid.IsFailure.Should().BeTrue();
    }
}

using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Tests.Support;
using FluentAssertions;

namespace AuthModule.Governance.Tests.Retention;

public sealed class RetentionServiceTests
{
    [Fact]
    public async Task RetentionInvoke_ShouldReturnDeterministicOutcomesForUnchangedState()
    {
        var sut = TestContextFactory.Create();
        var now = sut.RequestContext.Timestamp;
        var evidence = await sut.EvidenceService.CaptureAsync(new EvidenceCaptureRequest(
            EvidenceType: "SECURITY_EVENT",
            SubjectEntityType: "User",
            SubjectEntityId: Guid.NewGuid(),
            Payload: "{}",
            ControlMappingIds: [],
            RetentionExpiresAt: now.AddDays(-1)), sut.RequestContext);
        evidence.IsSuccess.Should().BeTrue();

        var first = await sut.RetentionService.InvokeAsync(new RetentionInvocationRequest("User"), sut.RequestContext);
        var second = await sut.RetentionService.InvokeAsync(new RetentionInvocationRequest("User"), sut.RequestContext);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        first.Value.Select(x => x.Outcome).Should().Equal(second.Value.Select(x => x.Outcome));
    }
}

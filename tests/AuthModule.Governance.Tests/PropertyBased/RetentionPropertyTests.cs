using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Tests.Support;
using FsCheck;
using FsCheck.Xunit;

namespace AuthModule.Governance.Tests.PropertyBased;

public sealed class RetentionPropertyTests
{
    [Property]
    public bool RetentionDecision_ShouldBeIdempotent_ForUnchangedState(NonNull<string> payload)
    {
        var sut = TestContextFactory.Create();
        sut.EvidenceService.CaptureAsync(new EvidenceCaptureRequest(
            EvidenceType: "SECURITY_EVENT",
            SubjectEntityType: "User",
            SubjectEntityId: Guid.NewGuid(),
            Payload: payload.Get,
            ControlMappingIds: [],
            RetentionExpiresAt: sut.RequestContext.Timestamp.AddDays(-1)), sut.RequestContext).GetAwaiter().GetResult();

        var first = sut.RetentionService.InvokeAsync(new RetentionInvocationRequest("User"), sut.RequestContext).GetAwaiter().GetResult();
        var second = sut.RetentionService.InvokeAsync(new RetentionInvocationRequest("User"), sut.RequestContext).GetAwaiter().GetResult();

        return first.IsSuccess && second.IsSuccess &&
               first.Value.Select(x => x.Outcome).SequenceEqual(second.Value.Select(x => x.Outcome));
    }
}

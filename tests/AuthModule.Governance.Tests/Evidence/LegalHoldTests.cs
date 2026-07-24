using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Domain;
using AuthModule.Governance.Tests.Support;
using FluentAssertions;

namespace AuthModule.Governance.Tests.Evidence;

public sealed class LegalHoldTests
{
    [Fact]
    public async Task DataSubjectExport_ShouldFailClosed_WhenLegalHoldExists()
    {
        var sut = TestContextFactory.Create();
        var subjectId = Guid.NewGuid();
        var capture = await sut.EvidenceService.CaptureAsync(new EvidenceCaptureRequest(
            EvidenceType: "ACCESS_REVIEW",
            SubjectEntityType: "User",
            SubjectEntityId: subjectId,
            Payload: "{}",
            ControlMappingIds: [],
            RetentionExpiresAt: DateTimeOffset.UtcNow.AddDays(30),
            LegalHoldActive: true,
            LegalHoldReason: "investigation"), sut.RequestContext);
        capture.IsSuccess.Should().BeTrue();

        var request = await sut.DataSubjectService.SubmitAsync(
            new DataSubjectRequestCommand(subjectId, DataSubjectRequestType.Export),
            sut.RequestContext);

        request.IsFailure.Should().BeTrue();
        request.Error.Code.Should().Be(AuthModule.Foundation.Domain.Primitives.DomainErrorCode.PolicyViolation);
    }
}

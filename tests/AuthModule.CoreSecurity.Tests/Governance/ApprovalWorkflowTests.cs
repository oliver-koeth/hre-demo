using AuthModule.CoreSecurity.Application.Contracts;
using AuthModule.CoreSecurity.Domain;
using AuthModule.CoreSecurity.Tests.Support;
using FluentAssertions;

namespace AuthModule.CoreSecurity.Tests.Governance;

public sealed class ApprovalWorkflowTests
{
    [Fact]
    [Trait("Security", "True")]
    public async Task Approval_ShouldRejectUnauthenticatedRequester()
    {
        var sut = await TestContextFactory.CreateAsync();
        var anonymousContext = sut.RequestContext with { UserId = null };

        var ticket = await sut.ApprovalWorkflowService.RequestApprovalAsync(
            new ApprovalRequest(Guid.NewGuid(), Guid.NewGuid(), "ROLE_PERMISSION_ASSIGN"),
            anonymousContext);

        ticket.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task Approval_ShouldRejectSelfApproval()
    {
        var sut = await TestContextFactory.CreateAsync();
        var actor = Guid.NewGuid();
        var requesterContext = sut.RequestContext with { UserId = actor };
        var ticket = await sut.ApprovalWorkflowService.RequestApprovalAsync(
            new ApprovalRequest(Guid.NewGuid(), Guid.NewGuid(), "ROLE_PERMISSION_ASSIGN"),
            requesterContext);

        var decision = await sut.ApprovalWorkflowService.DecideApprovalAsync(
            new ApprovalDecisionRequest(ticket.Value.TicketId, true, null),
            requesterContext);

        decision.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task Approval_ShouldTransition_ToApplied_WhenApprovedByAnotherActor()
    {
        var sut = await TestContextFactory.CreateAsync();
        var requestContext = sut.RequestContext with { UserId = Guid.NewGuid() };
        var ticket = await sut.ApprovalWorkflowService.RequestApprovalAsync(
            new ApprovalRequest(Guid.NewGuid(), Guid.NewGuid(), "ROLE_PERMISSION_ASSIGN"),
            requestContext);

        var decisionContext = sut.RequestContext with { UserId = Guid.NewGuid() };
        var decision = await sut.ApprovalWorkflowService.DecideApprovalAsync(
            new ApprovalDecisionRequest(ticket.Value.TicketId, true, null),
            decisionContext);

        decision.IsSuccess.Should().BeTrue();
        decision.Value.Status.Should().Be(ApprovalStatus.Applied);
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task Approval_Decision_ShouldRejectUnauthenticatedApprover()
    {
        var sut = await TestContextFactory.CreateAsync();
        var requesterContext = sut.RequestContext with { UserId = Guid.NewGuid() };
        var ticket = await sut.ApprovalWorkflowService.RequestApprovalAsync(
            new ApprovalRequest(Guid.NewGuid(), Guid.NewGuid(), "ROLE_PERMISSION_ASSIGN"),
            requesterContext);

        var anonymousContext = sut.RequestContext with { UserId = null };
        var decision = await sut.ApprovalWorkflowService.DecideApprovalAsync(
            new ApprovalDecisionRequest(ticket.Value.TicketId, true, null),
            anonymousContext);

        decision.IsFailure.Should().BeTrue();
    }
}

using System.Net;
using System.Net.Http.Json;
using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;

namespace AuthModule.ServiceHost.Tests.Api;

public sealed class ApprovalSecurityTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    [Fact]
    [Trait("Security", "True")]
    public async Task RequestApproval_ShouldReject_WhenActorHeaderMissing()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/core-security/governance/approvals", new
        {
            roleId = Guid.NewGuid(),
            permissionId = Guid.NewGuid(),
            changeType = "ROLE_PERMISSION_ASSIGN",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    [Trait("Security", "True")]
    public async Task DecideApproval_ShouldReject_WhenActorHeaderMissing()
    {
        var client = factory.CreateClient();
        var requester = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/core-security/governance/approvals")
        {
            Content = JsonContent.Create(new
            {
                roleId = Guid.NewGuid(),
                permissionId = Guid.NewGuid(),
                changeType = "ROLE_PERMISSION_ASSIGN",
            }),
        };
        request.Headers.Add("X-Actor-UserId", requester);

        var createResponse = await client.SendAsync(request);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createResponse.Content.ReadFromJsonAsync<ApprovalTicketDto>();
        created.Should().NotBeNull();

        var decideResponse = await client.PostAsJsonAsync("/api/core-security/governance/approvals/decide", new
        {
            ticketId = created!.TicketId,
            approved = true,
            rejectionReason = (string?)null,
        });

        decideResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record ApprovalTicketDto(Guid TicketId);
}

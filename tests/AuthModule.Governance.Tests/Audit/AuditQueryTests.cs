using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Tests.Support;
using FluentAssertions;

namespace AuthModule.Governance.Tests.Audit;

public sealed class AuditQueryTests
{
    [Fact]
    public async Task QuerySecurityEvents_ShouldReturnPagedData()
    {
        var sut = TestContextFactory.Create();
        var result = await sut.AuditQueryService.QuerySecurityEventsAsync(
            new AuditQueryRequest(null, null, null, null, Page: 1, PageSize: 10),
            sut.RequestContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().BeGreaterThan(0);
        result.Value.Events.Should().NotBeEmpty();
    }
}

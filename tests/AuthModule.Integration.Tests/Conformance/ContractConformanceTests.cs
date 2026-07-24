using AuthModule.Integration.Tests.Support;
using FluentAssertions;

namespace AuthModule.Integration.Tests.Conformance;

public sealed class ContractConformanceTests
{
    [Fact]
    public async Task Conformance_ShouldReportBlockingFinding_WhenErrorCodeOrCorrelationIdMissing()
    {
        var sut = TestContextFactory.Create(root =>
        {
            File.WriteAllText(
                Path.Combine(root, "src", "AuthModule", "CoreSecurity", "Api", "CoreSecurityProblemDetails.cs"),
                "var details = new ProblemDetails(); details.Extensions[\"errorCode\"] = \"X\";");
        });
        var request = TestContextFactory.BuildRequest();

        var result = await sut.GateService.ExecuteAsync(request, sut.RequestContext);

        result.IsSuccess.Should().BeTrue();
        result.Value.ConformanceFindings.Should().Contain(x => x.IsBlocking);
        result.Value.Decision.BlockingFindings.Should().Contain(x => x.Category == "ContractConformance");
    }
}

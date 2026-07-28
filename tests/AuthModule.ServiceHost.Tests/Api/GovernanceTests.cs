using AuthModule.ServiceHost.Tests.Support;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AuthModule.ServiceHost.Tests.Api;

public sealed class GovernanceTests(ServiceHostFactory factory) : IClassFixture<ServiceHostFactory>
{
    [Fact]
    public async Task AuditSecurityEvents_ShouldReturn200_WithResultArray()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/governance/audit/security-events");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("events", out _).Should().BeTrue();
    }

    [Fact]
    public async Task CaptureEvidence_ShouldReturn200_WithEvidenceId()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/governance/evidence", new
        {
            evidenceType = "SECURITY_EVENT",
            subjectEntityType = "User",
            subjectEntityId = Guid.NewGuid(),
            payload = "Integration test evidence payload",
            controlMappingIds = new[] { "CTRL-001" },
            retentionExpiresAt = (DateTimeOffset?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("evidenceId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task ExportEvidence_ShouldReturn200()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/governance/evidence/export", new
        {
            evidenceType = (string?)null,
            subjectEntityType = (string?)null,
            subjectEntityId = (Guid?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateIncident_ShouldReturn200_WithIncidentId()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/governance/incidents", new
        {
            title = "Integration test incident",
            severity = "High",
            serviceImpact = "auth-module",
            breachReportable = false,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("incidentId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task AdvanceIncidentStatus_ShouldReturn200()
    {
        var client = factory.CreateClient();

        // Create incident first
        var createResp = await client.PostAsJsonAsync("/api/governance/incidents", new
        {
            title = "Status advance test",
            severity = "Low",
            serviceImpact = "auth-module",
            breachReportable = false,
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var incidentId = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync())
            .RootElement.GetProperty("incidentId").GetGuid();

        var advanceResp = await client.PostAsJsonAsync("/api/governance/incidents/status", new
        {
            incidentId,
            targetStatus = "Investigating",
        });

        advanceResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AppendBackupMetadata_ShouldReturn200_WithBackupId()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/governance/backups", new
        {
            backupType = "Full",
            storePath = "/backups/test-backup.tar.gz",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("backupId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateBackupStatus_ShouldReturn200()
    {
        var client = factory.CreateClient();

        // Create backup record first
        var createResp = await client.PostAsJsonAsync("/api/governance/backups", new
        {
            backupType = "Incremental",
            storePath = "/backups/inc-backup.tar.gz",
        });
        var backupId = JsonDocument.Parse(await createResp.Content.ReadAsStringAsync())
            .RootElement.GetProperty("backupId").GetGuid();

        var statusResp = await client.PostAsJsonAsync("/api/governance/backups/status", new
        {
            backupId,
            targetStatus = "Completed",
        });

        statusResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

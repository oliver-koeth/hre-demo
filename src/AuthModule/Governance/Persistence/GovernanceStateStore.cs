using System.Collections.Concurrent;
using AuthModule.Governance.Domain;

namespace AuthModule.Governance.Persistence;

public interface IGovernanceStateStore
{
    void SaveEvidence(EvidenceRecord record);
    IReadOnlyList<EvidenceRecord> QueryEvidence(string? evidenceType, string? subjectEntityType, Guid? subjectEntityId);
    bool HasLegalHold(Guid subjectEntityId, out string reason);

    void SaveDataSubjectRequest(DataSubjectRequestRecord request);
    DataSubjectRequestRecord? GetDataSubjectRequest(Guid requestId);

    void SaveRetentionRule(RetentionRuleRecord rule);
    IReadOnlyList<RetentionRuleRecord> GetActiveRetentionRules(string entityType);
    void SaveRetentionDecision(LifecycleDecisionRecord decision);
    IReadOnlyList<LifecycleDecisionRecord> GetRetentionDecisions();
    RetentionFingerprintRecord? GetFingerprint(Guid ruleId, Guid entityId);
    void SaveFingerprint(RetentionFingerprintRecord record);

    void SaveIncident(GovernanceIncidentRecord incident);
    GovernanceIncidentRecord? GetIncident(Guid incidentId);

    void SaveBackupMetadata(BackupMetadataRecord record);
    BackupMetadataRecord? GetBackupMetadata(Guid backupId);
}

public sealed class InMemoryGovernanceStateStore : IGovernanceStateStore
{
    private readonly ConcurrentDictionary<Guid, EvidenceRecord> _evidence = new();
    private readonly ConcurrentDictionary<Guid, DataSubjectRequestRecord> _subjectRequests = new();
    private readonly ConcurrentDictionary<Guid, RetentionRuleRecord> _rules = new();
    private readonly ConcurrentDictionary<Guid, LifecycleDecisionRecord> _decisions = new();
    private readonly ConcurrentDictionary<string, RetentionFingerprintRecord> _fingerprints = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<Guid, GovernanceIncidentRecord> _incidents = new();
    private readonly ConcurrentDictionary<Guid, BackupMetadataRecord> _backups = new();

    public void SaveEvidence(EvidenceRecord record) => _evidence[record.EvidenceId] = record;

    public IReadOnlyList<EvidenceRecord> QueryEvidence(string? evidenceType, string? subjectEntityType, Guid? subjectEntityId)
    {
        IEnumerable<EvidenceRecord> query = _evidence.Values;
        if (!string.IsNullOrWhiteSpace(evidenceType))
        {
            query = query.Where(x => string.Equals(x.EvidenceType, evidenceType, StringComparison.Ordinal));
        }

        if (!string.IsNullOrWhiteSpace(subjectEntityType))
        {
            query = query.Where(x => string.Equals(x.SubjectEntityType, subjectEntityType, StringComparison.Ordinal));
        }

        if (subjectEntityId is not null)
        {
            query = query.Where(x => x.SubjectEntityId == subjectEntityId.Value);
        }

        return query.OrderBy(x => x.CapturedAt).ToList();
    }

    public bool HasLegalHold(Guid subjectEntityId, out string reason)
    {
        var held = _evidence.Values.FirstOrDefault(x => x.SubjectEntityId == subjectEntityId && x.LegalHoldActive);
        if (held is null)
        {
            reason = string.Empty;
            return false;
        }

        reason = held.LegalHoldReason ?? "Legal hold active.";
        return true;
    }

    public void SaveDataSubjectRequest(DataSubjectRequestRecord request) => _subjectRequests[request.RequestId] = request;

    public DataSubjectRequestRecord? GetDataSubjectRequest(Guid requestId) => _subjectRequests.GetValueOrDefault(requestId);

    public void SaveRetentionRule(RetentionRuleRecord rule) => _rules[rule.RuleId] = rule;

    public IReadOnlyList<RetentionRuleRecord> GetActiveRetentionRules(string entityType) =>
        _rules.Values.Where(x => x.IsActive && string.Equals(x.EntityType, entityType, StringComparison.Ordinal)).ToList();

    public void SaveRetentionDecision(LifecycleDecisionRecord decision) => _decisions[decision.DecisionId] = decision;

    public IReadOnlyList<LifecycleDecisionRecord> GetRetentionDecisions() => _decisions.Values.ToList();

    public RetentionFingerprintRecord? GetFingerprint(Guid ruleId, Guid entityId) =>
        _fingerprints.GetValueOrDefault($"{ruleId}:{entityId}");

    public void SaveFingerprint(RetentionFingerprintRecord record) =>
        _fingerprints[$"{record.RuleId}:{record.EntityId}"] = record;

    public void SaveIncident(GovernanceIncidentRecord incident) => _incidents[incident.IncidentId] = incident;

    public GovernanceIncidentRecord? GetIncident(Guid incidentId) => _incidents.GetValueOrDefault(incidentId);

    public void SaveBackupMetadata(BackupMetadataRecord record) => _backups[record.BackupId] = record;

    public BackupMetadataRecord? GetBackupMetadata(Guid backupId) => _backups.GetValueOrDefault(backupId);
}

using System.Security.Cryptography;
using System.Text;
using AuthModule.Foundation.Domain.Primitives;
using AuthModule.Governance.Application.Common;
using AuthModule.Governance.Application.Contracts;
using AuthModule.Governance.Domain;
using AuthModule.Governance.Persistence;

namespace AuthModule.Governance.Application.Retention;

public sealed class RetentionService(
    IGovernanceStateStore stateStore,
    IAlertService alertService) : IRetentionService
{
    public async Task<Result<IReadOnlyList<LifecycleDecisionRecord>, DomainError>> InvokeAsync(RetentionInvocationRequest request, RequestContext context)
    {
        var rules = stateStore.GetActiveRetentionRules(request.EntityType);
        if (rules.Count == 0)
        {
            await alertService.EmitRetentionFailureAsync(request.EntityType, "No active retention rule for requested entity type.", context);
            return Result<IReadOnlyList<LifecycleDecisionRecord>, DomainError>.Failure(ErrorFactory.Validation("No active retention rule for entity type.", context));
        }

        var evidenceRecords = stateStore.QueryEvidence(null, request.EntityType, null);
        var output = new List<LifecycleDecisionRecord>();

        foreach (var rule in rules)
        {
            foreach (var record in evidenceRecords)
            {
                var fingerprintValue = BuildFingerprint(rule, record, context.Timestamp);
                var existing = stateStore.GetFingerprint(rule.RuleId, record.EvidenceId);
                if (existing is not null && string.Equals(existing.Fingerprint, fingerprintValue, StringComparison.Ordinal))
                {
                    output.Add(new LifecycleDecisionRecord
                    {
                        DecisionId = Guid.NewGuid(),
                        RuleId = rule.RuleId,
                        EntityType = request.EntityType,
                        EntityId = record.EvidenceId,
                        EvaluatedAt = context.Timestamp,
                        Action = rule.Action,
                        Outcome = existing.LastOutcome,
                        CorrelationId = context.CorrelationId,
                        Version = 0,
                    });
                    continue;
                }

                var outcome = ComputeOutcome(rule, record, context.Timestamp, out var reason);
                var decision = new LifecycleDecisionRecord
                {
                    DecisionId = Guid.NewGuid(),
                    RuleId = rule.RuleId,
                    EntityType = request.EntityType,
                    EntityId = record.EvidenceId,
                    EvaluatedAt = context.Timestamp,
                    Action = rule.Action,
                    Outcome = outcome,
                    BlockReason = reason,
                    CorrelationId = context.CorrelationId,
                    Version = 0,
                };
                stateStore.SaveRetentionDecision(decision);
                stateStore.SaveFingerprint(new RetentionFingerprintRecord
                {
                    FingerprintId = existing?.FingerprintId ?? Guid.NewGuid(),
                    RuleId = rule.RuleId,
                    EntityId = record.EvidenceId,
                    Fingerprint = fingerprintValue,
                    LastOutcome = outcome,
                    UpdatedAt = context.Timestamp,
                    Version = (existing?.Version ?? -1) + 1,
                });
                output.Add(decision);
            }
        }

        return Result<IReadOnlyList<LifecycleDecisionRecord>, DomainError>.Success(output);
    }

    private static LifecycleOutcome ComputeOutcome(
        RetentionRuleRecord rule,
        EvidenceRecord record,
        DateTimeOffset now,
        out string? reason)
    {
        if (record.LegalHoldActive)
        {
            reason = record.LegalHoldReason ?? "Legal hold active.";
            return LifecycleOutcome.BlockedByHold;
        }

        if (record.RetentionExpiresAt is null || record.RetentionExpiresAt > now)
        {
            reason = null;
            return LifecycleOutcome.Skipped;
        }

        reason = null;
        return LifecycleOutcome.Applied;
    }

    private static string BuildFingerprint(RetentionRuleRecord rule, EvidenceRecord record, DateTimeOffset now)
    {
        var input = $"{rule.RuleId}|{record.EvidenceId}|{rule.Action}|{record.RetentionExpiresAt:O}|{record.LegalHoldActive}|{record.LegalHoldReason}|{now:yyyy-MM-dd}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}

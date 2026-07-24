using System.Collections.Concurrent;
using AuthModule.Integration.Application.Contracts;
using AuthModule.Integration.Domain;

namespace AuthModule.Integration.Persistence;

public sealed class InMemoryIntegrationStateStore : IIntegrationStateStore
{
    private readonly ConcurrentQueue<GateDecisionRecord> _decisions = new();
    private readonly ConcurrentDictionary<Guid, BlockingFindingRecord> _openFindings = new();

    public void SaveGateDecision(GateDecisionRecord decision) => _decisions.Enqueue(decision);

    public GateDecisionRecord? GetLatestDecision() =>
        _decisions.LastOrDefault();

    public void ReplaceOpenBlockingFindings(IReadOnlyList<BlockingFindingRecord> findings)
    {
        _openFindings.Clear();
        foreach (var finding in findings.Where(x => x.IsOpen))
        {
            _openFindings[finding.BlockingFindingId] = finding;
        }
    }

    public IReadOnlyList<BlockingFindingRecord> GetOpenBlockingFindings() =>
        _openFindings.Values.OrderBy(x => x.CreatedAt).ToList();
}

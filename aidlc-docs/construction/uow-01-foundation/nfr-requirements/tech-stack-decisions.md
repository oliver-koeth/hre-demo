# Tech Stack Decisions — UOW-01 NFR Requirements

## Decision Summary

| Decision | Selected | Rationale | Risk/Control |
|---|---|---|---|
| Runtime | .NET 10 preview | User-selected for latest runtime features in early implementation | Not for production promotion; mandatory runtime upgrade gate before go-live |
| JSON library | System.Text.Json | Built-in, high-performance, low dependency surface, good AOT/source-gen compatibility | Configure explicit serialization options to avoid behavior drift |
| Logging | Serilog | Mature structured logging ecosystem and strong enrichers for correlation context | Enforce redaction policy; route to centralized sink in downstream stages |
| Test runner | xUnit + FluentAssertions | Standard .NET testing stack with strong ecosystem and readable assertions | Keep test categories explicit (example vs property-based) |
| PBT framework | FsCheck | Meets PBT-09 requirements: generators, shrinking, seed reproducibility, xUnit integration | Add deterministic seed logging in CI and failure output |
| Read latency target | P99 < 100 ms | Balanced target accounting for crypto and JSON parse overhead | Track histogram metrics and adjust store partitioning if breached |
| Write latency target | P99 < 250 ms | Includes encryption + HMAC + atomic rename path | Monitor file size growth and compaction/migration overhead |

## Framework Capability Check (PBT-09)

FsCheck provides required capabilities:
- Custom generators for domain objects
- Automatic shrinking of failing cases
- Seed-based reproducibility
- Integration with existing runner (xUnit)

Conclusion: **PBT-09 compliant for this stage**.

## Implementation Guidance for Construction Stages

1. Pin NuGet package versions for Serilog, FsCheck, xUnit, and FluentAssertions.
2. Configure Serilog enrichers for `CorrelationId`, `UserId`, and request metadata.
3. Add perf tests that assert read/write P99 targets for representative store sizes.
4. Treat runtime version as an explicit release gate item before production deployment.

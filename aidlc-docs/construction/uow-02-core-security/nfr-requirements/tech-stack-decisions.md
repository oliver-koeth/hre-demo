# Tech Stack Decisions — UOW-02 NFR Requirements

## Decision Summary

| Area | Decision | Rationale |
|---|---|---|
| Login throughput target | 50 req/s | Conservative baseline aligned to early-stage deployment capacity |
| Token validation SLO | P99 < 50 ms | Strong responsiveness requirement for protected paths |
| Lockout alert threshold | 5 lockouts/account in 10 min | Early abuse detection for credential attack patterns |
| MFA provider strategy | External provider integration | Aligns sensitive admin controls to production-grade MFA ecosystem |
| Approval operation availability | 99.5% monthly | Fits governed-change criticality without over-engineering in V1 |
| Disable-user revocation consistency | Strong consistency | Immediate access termination is a hard security expectation |
| Abuse protection | Per-IP + per-account rate limits | Mitigates shared-IP noise and account-targeted attacks together |
| JWT signing | HMAC symmetric in V1 | Matches current architecture and key handling model |
| Event retention | 90 days minimum | Meets baseline monitoring/compliance retention floor |
| PBT scope | Auth/token invariants only | Prioritizes highest-risk runtime correctness properties for UOW-02 |

---

## Additional Implementation Notes

1. External MFA integration should be abstracted behind a provider interface to keep future portability.
2. Rate-limiting policy should expose configurable thresholds for login and validation routes independently.
3. Token validation latency budget should include signature verification, claims checks, account status lookup, and token-version check.
4. Approval availability SLO should be tracked separately from generic admin endpoint availability.

---

## PBT-09 Compatibility Note

UOW-02 continues the selected .NET PBT framework (**FsCheck**) from UOW-01.
This satisfies PBT framework consistency and avoids multi-framework complexity in the same service.

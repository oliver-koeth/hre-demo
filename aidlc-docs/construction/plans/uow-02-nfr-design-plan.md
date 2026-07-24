# UOW-02 NFR Design Plan

## Unit: UOW-02 Core Auth and Authorization Control Plane

### Plan Checklist
- [x] Collect and validate answers to NFR design questions
- [x] Define resilience patterns for auth, token validation, and approval workflows
- [x] Define scalability/performance patterns for login and protected endpoint paths
- [x] Define security patterns for MFA, rate limits, token signing, and abuse detection
- [x] Define logical components and interaction boundaries
- [x] Generate `nfr-design-patterns.md`
- [x] Generate `logical-components.md`

---

## NFR Design Questions

### Q1: Login Throttling Strategy
How should per-IP and per-account rate limits be enforced?

A. Fixed-window counters with hard deny on threshold breach  
B. Sliding-window limits with burst allowance and cooldown  
C. Token-bucket algorithm with adaptive refill rates  
X. Other  

[Answer]: A

---

### Q2: Token Validation Performance Pattern
What primary pattern should keep token validation under P99 < 50 ms?

A. Local in-memory cache for active user status + token version (short TTL)  
B. Always fetch user state from persistence per request (no cache)  
C. Hybrid: local cache with async background refresh and fallback to direct lookup  
X. Other  

[Answer]: A

---

### Q3: MFA Integration Pattern
How should external MFA provider integration be modeled?

A. Synchronous provider call per challenge verification  
B. Pluggable provider interface + adapter (default provider implementation)  
C. Event-driven challenge verification via async callback/webhook pipeline  
X. Other  

[Answer]: A

---

### Q4: Approval Workflow Reliability Pattern
How should 99.5% availability for governed approval operations be achieved?

A. Synchronous persistence with bounded retries and fail-closed response  
B. Command queue with durable retry worker and idempotent command handling  
C. Multi-path fallback (queue + direct write) with conflict arbitration  
X. Other  

[Answer]: A

---

### Q5: Disable User Consistency Pattern
How should strong consistency for immediate access termination be enforced?

A. Single transaction-style sequence: disable user + increment token version before response  
B. Disable user first, then best-effort token version update with retry  
C. Central revocation table checked before token version validation  
X. Other  

[Answer]: A

---

### Q6: Lockout Alerting Pattern
How should lockout threshold alerts (5 in 10 minutes) be implemented?

A. In-process rolling counter with immediate alert emission  
B. Stream lockout events to alerting engine; threshold evaluation externalized  
C. Both in-process immediate guardrail and external correlation pipeline  
X. Other  

[Answer]: A

---

### Q7: JWT Key Management Pattern (HMAC)
How should signing key lifecycle be handled for V1?

A. Single active HMAC key with planned manual rotation procedure  
B. Key ring with active + previous keys for seamless rotation  
C. Key versioning in token header with key-provider lookup per version  
X. Other  

[Answer]: A

---

### Q8: Error Response Reliability Pattern
How should RFC7807 + correlation guarantees be enforced uniformly?

A. Global exception/response middleware with centralized mapper  
B. Per-endpoint explicit mapping logic  
C. Service-layer mappers plus API middleware fallback  
X. Other  

[Answer]: A

---

### Q9: Security Event Retention/Archival Pattern
How should 90-day minimum retention be implemented operationally?

A. Hot retention in primary store with periodic archival snapshots  
B. Tiered storage policy (hot + cold archive) from day 1  
C. Retention marker only in V1, archival process deferred  
X. Other  

[Answer]:A

---

### Q10: Logical Component Partitioning
How should UOW-02 runtime be partitioned into logical components?

A. `AuthService`, `TokenValidationService`, `AuthorizationService`, `ApprovalService`, `RateLimitService`, `MfaService`  
B. `AuthControlPlaneService` monolith + internal modules  
C. Hybrid: auth/validation merged, approval and MFA separate  
X. Other  

[Answer]: B

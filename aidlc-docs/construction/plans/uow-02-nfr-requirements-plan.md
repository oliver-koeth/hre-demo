# UOW-02 NFR Requirements Plan

## Unit: UOW-02 Core Auth and Authorization Control Plane

## Context Summary
Inherited baseline from prior stages:
- C# + Docker runtime
- JSON persistence boundary from UOW-01
- Serilog + OpenTelemetry stack
- Warm standby DR strategy
- Security-first + deny-by-default policy

This plan captures UOW-02 specific NFR targets for auth, authorization, and governed admin operations.

---

### Plan Checklist
- [x] Collect and validate NFR answers
- [x] Generate `nfr-requirements.md`
- [x] Generate `tech-stack-decisions.md`

---

## NFR Questions

### Q1: Login Throughput Target
What sustained throughput should `/auth/login` target in V1?

A. 50 req/s  
B. 100 req/s  
C. 250 req/s  
X. Other  

[Answer]: A

---

### Q2: Token Validation Latency SLO
What P99 latency target should apply to token validation on protected endpoints?

A. < 20 ms  
B. < 50 ms  
C. < 100 ms  
X. Other  

[Answer]: B

---

### Q3: Lockout Event Alert Threshold
When should repeated lockouts trigger an operational security alert?

A. 5 lockouts for same account within 10 minutes  
B. 10 lockouts for same account within 10 minutes  
C. 20 lockouts for same account within 30 minutes  
X. Other  

[Answer]: A

---

### Q4: MFA Provider Strategy (Sensitive Admin Step-Up)
How should MFA challenge verification be handled in V1?

A. Internal TOTP verifier in service (no external dependency)  
B. External MFA provider integration  
C. Pluggable interface with internal default and external-ready adapter  
X. Other  

[Answer]: B (e.g. Microsoft Authenticator)

---

### Q5: Approval Workflow Reliability
What availability target should apply to governed role-permission approval operations?

A. 99.5% monthly  
B. 99.9% monthly  
C. 99.95% monthly  
X. Other  

[Answer]: A

---

### Q6: Auth Session Revocation Consistency
What consistency model should be enforced for immediate access termination on user disable?

A. Strong consistency in-process before returning disable success  
B. Eventual consistency (up to 60 seconds)  
C. Eventual consistency (up to 5 minutes)  
X. Other  

[Answer]: A

---

### Q7: API Abuse Protection
What request-rate protection should be applied in V1?

A. Per-IP + per-account rate limits on login and token-validation endpoints  
B. Per-IP only  
C. WAF/gateway-level only (no in-service throttling)  
X. Other  

[Answer]: A

---

### Q8: Cryptographic Signing Strategy for JWT
For this unit, should token signing remain:

A. HMAC symmetric signing (current baseline)  
B. Asymmetric signing (RSA/ECDSA)  
C. Pluggable, with HMAC default in V1  
X. Other  

[Answer]: A

---

### Q9: Audit Retention for Auth Events
What minimum retention should apply for auth/admin security events in UOW-02 outputs?

A. 90 days  
B. 180 days  
C. 365 days  
X. Other  

[Answer]: A

---

### Q10: PBT-09 Scope for UOW-02
Which property-based test scope is required in this unit’s downstream code generation?

A. Auth/token invariants only  
B. Auth/token invariants + approval workflow state machine sequences  
C. Full scope: auth/token + approval workflow + permission resolution oracle checks  
X. Other  

[Answer]: A

# UOW-02 Functional Design Plan

## Unit: UOW-02 Core Auth and Authorization Control Plane

### Plan Checklist
- [x] Collect and validate answers to clarifying questions
- [x] Define authentication business logic model (US-01, US-02, US-03)
- [x] Define authorization and admin business logic model (US-04, US-05, US-06)
- [x] Define SoD approval workflow state machine and transitions
- [x] Define API request/response contracts and validation rules
- [x] Define business rules and decision tables
- [x] Identify PBT-01 testable properties for core security behaviors
- [x] Generate `domain-entities.md`
- [x] Generate `business-rules.md`
- [x] Generate `business-logic-model.md`

---

## Clarifying Questions

### Q1: Login Identifier Policy
Which identifier(s) should the login endpoint accept in V1?

A. Username only  
B. Username or email  
C. Email only  
X. Other  

[Answer]: C

---

### Q2: Password Hash Verification
How should credential verification handle algorithm compatibility in V1?

A. Accept Argon2id and BCrypt for verification; all new credentials issued as Argon2id  
B. Accept only Argon2id; reject BCrypt credentials  
C. Accept Argon2id and BCrypt and preserve existing algorithm on password rotation  
X. Other  

[Answer]: A

---

### Q3: Failed Login Protection
What lockout strategy should be used for repeated failed login attempts?

A. Hard lockout after max attempts; unlock after configured lockout duration  
B. Progressive delay first, then hard lockout after threshold  
C. Hard lockout only for privileged/admin accounts; delay for standard accounts  
X. Other  

[Answer]: A

---

### Q4: Token Validation Strictness
For server-side token validation, which checks are mandatory on every protected request?

A. Signature + expiry + issuer + audience + account status + token version  
B. Signature + expiry + issuer + audience only  
C. Signature + expiry + issuer + audience + account status (no token versioning in V1)  
X. Other  

[Answer]: A

---

### Q5: Privileged Session Controls
Which step-up control should apply for sensitive admin operations?

A. Re-authentication with password within configurable recent-auth window  
B. MFA challenge for every sensitive admin operation  
C. Re-authentication + MFA for sensitive admin operations  
X. Other  

[Answer]: B

---

### Q6: Permission Evaluation Order
How should effective authorization be evaluated?

A. Explicit deny (if introduced later) > explicit allow via role-permission > default deny  
B. Allow if any assigned role grants permission; otherwise deny  
C. Role priority model (higher-priority roles override lower ones)  
X. Other  

[Answer]: B

---

### Q7: SoD Governance Scope
Which admin changes require two-person approval in V1?

A. Role-permission assignment/revocation only  
B. Role-permission + privileged user role assignment/revocation  
C. All admin mutations (user, role, permission, assignment)  
X. Other  

[Answer]: A

---

### Q8: Approval Workflow Timeout
What should happen to pending approval tickets after timeout?

A. Auto-expire and require fresh request  
B. Remain pending until explicitly approved/rejected  
C. Auto-reject and emit high-priority security event  
X. Other  

[Answer]: B

---

### Q9: User Lifecycle Operations
For user disable/delete operations, what behavior should authorization enforce?

A. Disable only in V1 (no hard delete), with immediate access termination  
B. Disable + soft-delete supported in V1, both governed actions  
C. Full delete allowed for non-privileged users only  
X. Other  

[Answer]: A

---

### Q10: Permission Namespace Convention
How should permission keys be modeled for admin management and endpoint checks?

A. `resource:action` string format (e.g., `users:read`)  
B. Structured fields (`resource`, `action`) and generated composite key for checks  
C. Route-based permissions (HTTP method + route pattern)  
X. Other  

[Answer]: A

---

### Q11: API Error Contract
Which error response model should external API endpoints expose?

A. Domain `Result` mapped to HTTP problem details with correlation ID and stable error code  
B. Custom API envelope (`success`, `errorCode`, `message`, `correlationId`)  
C. Minimal HTTP status + message only  
X. Other  

[Answer]:A

---

### Q12: PBT Focus Priority for UOW-02
Which property categories should be prioritized in Functional Design outputs for UOW-02?

A. Invariants + idempotency + round-trip token validation behaviors  
B. Invariants + stateful command-sequence properties for approval workflow  
C. All of A and B, plus oracle checks for permission resolution model  
X. Other  

[Answer]: A

# UOW-02 Infrastructure Design Plan

## Unit: UOW-02 Core Auth and Authorization Control Plane

### Plan Checklist
- [x] Collect and validate infrastructure answers
- [x] Map UOW-02 logical components to concrete infrastructure services
- [x] Define deployment architecture for auth control plane runtime
- [x] Define MFA integration and rate-limiting infrastructure dependencies
- [x] Generate `infrastructure-design.md`
- [x] Generate `deployment-architecture.md`

---

## Infrastructure Questions

### Q1: Runtime Hosting for UOW-02
Where should UOW-02 auth control plane run in this phase?

A. Same local Docker Compose stack as UOW-01  
B. Dedicated local Compose stack for UOW-02 only  
C. Cloud-hosted container runtime now (dev environment)  
X. Other  

[Answer]: A

---

### Q2: Network Exposure Model
How should auth and protected endpoint traffic be exposed?

A. Internal-only exposure in local network (no public ingress)  
B. Public ingress via reverse proxy with TLS termination  
C. API gateway fronting auth service with explicit route policies  
X. Other  

[Answer]: A

---

### Q3: External MFA Connectivity
How should synchronous external MFA provider calls be routed?

A. Direct outbound HTTPS from auth service with egress restrictions  
B. Outbound through dedicated gateway/proxy for provider allowlisting  
C. Local mock adapter only in this phase; no external call path yet  
X. Other  

[Answer]: C

---

### Q4: Rate Limit State Storage
Where should per-IP/per-account fixed-window counters be stored?

A. In-memory in service instance (single-instance assumption)  
B. Shared distributed cache (e.g., Redis)  
C. Persistent store table/file with periodic compaction  
X. Other  

[Answer]: A

---

### Q5: Token Validation Cache Placement
How should short-TTL user-status/token-version cache be deployed?

A. In-process memory cache only  
B. Shared external cache service  
C. Hybrid in-process primary + external cache fallback  
X. Other  

[Answer]: A

---

### Q6: Approval Workflow Persistence Reliability
For synchronous approval persistence with bounded retries, what infrastructure support is required?

A. Existing UOW-01 JSON store volume only  
B. Dedicated approval store partition + independent backup schedule  
C. Migrate approval persistence to separate data service now  
X. Other  

[Answer]: A

---

### Q7: Security Alert Delivery Channel
How should immediate lockout threshold alerts be delivered?

A. Structured log event only (consumed by log pipeline)  
B. Log event + direct notification hook (webhook/email/incident tool)  
C. In-app dashboard queue only  
X. Other  

[Answer]: A

---

### Q8: Shared Infrastructure Strategy with UOW-01
How should UOW-02 reuse or isolate infrastructure from UOW-01?

A. Reuse shared network/observability/secrets baseline; isolate auth runtime config and data paths  
B. Fully isolated stack for UOW-02  
C. Shared everything including data paths  
X. Other  

[Answer]: A

---

### Q9: Environment Topology for UOW-02
How many environments should be actively defined for this unit now?

A. Dev only  
B. Dev + Staging  
C. Dev + Staging + Production  
X. Other  

[Answer]: A

---

### Q10: Runtime Governance Gate
How should preview-runtime risk be handled for UOW-02 deployment artifacts?

A. Same as UOW-01: allow with explicit approval note + risk warning  
B. Block promotion unless runtime is supported GA/LTS  
C. Dual manifest strategy: preview for dev, supported runtime for staging/prod  
X. Other  

[Answer]: A

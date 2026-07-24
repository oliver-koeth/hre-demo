# UOW-03 Infrastructure Design Plan

## Unit: UOW-03 Governance and Evidence Domains

### Plan Checklist
- [x] Collect and validate infrastructure answers
- [x] Map UOW-03 logical components to concrete infrastructure services
- [x] Define deployment architecture for GovernanceService runtime
- [x] Define retention, incident durability, and alerting infrastructure dependencies
- [x] Generate `infrastructure-design.md`
- [x] Generate `deployment-architecture.md`

---

## Infrastructure Questions

### Q1: Runtime Hosting for UOW-03
Where should GovernanceService run in this phase?

A. Same local Docker Compose stack as UOW-01/UOW-02  
B. Dedicated local Compose stack for UOW-03 only  
C. Cloud-hosted container runtime now (dev environment)  
X. Other  

[Answer]: A

---

### Q2: Audit/Evidence API Exposure Model
How should governance query/export endpoints be exposed?

A. Internal-only exposure on local network (no public ingress)  
B. Public ingress via reverse proxy + TLS termination  
C. API gateway fronting governance routes with explicit policies  
X. Other  

[Answer]: A

---

### Q3: Retention Invocation Execution Surface
How should manual retention invocation be executed in infrastructure?

A. Synchronous API-triggered operation in GovernanceService process  
B. API command writes to queue; worker executes asynchronously  
C. Operator CLI inside container environment triggers internal command  
X. Other  

[Answer]: A

---

### Q4: Idempotence Fingerprint Persistence
Where should retention decision fingerprints be persisted?

A. Existing JSON store volume (governance namespace)  
B. Dedicated lightweight key-value store (e.g., Redis/SQLite sidecar)  
C. In-memory only (recomputed on restart)  
X. Other  

[Answer]: A

---

### Q5: Incident Durability Enforcement Support
What infrastructure support should back fsync-before-success incident writes?

A. Existing single JSON store volume with strict write semantics  
B. Primary volume + immediate mirrored local volume copy  
C. Primary volume + async replica channel  
X. Other  

[Answer]: A

---

### Q6: Backup Metadata Freshness Mechanism
How should “queryable within 30s” for backup metadata be supported?

A. Immediate append + in-process cache invalidation only  
B. Periodic poller (<=30s) refreshing read model  
C. Event bus projector feeding query model  
X. Other  

[Answer]: A

---

### Q7: Alert Delivery Channel
How should immediate alerts (retention failures / blocked bypass attempts) be delivered?

A. Structured log/security event only (existing pipeline)  
B. Structured log + direct notification hook (webhook/email/incident tool)  
C. Dedicated alert queue + consumer service  
X. Other  

[Answer]: A

---

### Q8: Shared Infrastructure Strategy with Prior UOWs
How should UOW-03 reuse or isolate infrastructure from UOW-01/UOW-02?

A. Reuse shared network/observability/secrets baseline; isolate governance config/data paths  
B. Fully isolated infrastructure stack for UOW-03  
C. Shared everything including data paths  
X. Other  

[Answer]: A

---

### Q9: Environment Topology for UOW-03
How many environments should be actively defined now?

A. Dev only  
B. Dev + Staging  
C. Dev + Staging + Production  
X. Other  

[Answer]:A

---

### Q10: Runtime Governance Gate
How should preview-runtime risk be handled for UOW-03 deployment artifacts?

A. Same as prior UOWs: allow with explicit approval note + risk warning  
B. Block promotion unless runtime is supported GA/LTS  
C. Dual manifest strategy: preview for dev, supported runtime for staging/prod  
X. Other  

[Answer]:A

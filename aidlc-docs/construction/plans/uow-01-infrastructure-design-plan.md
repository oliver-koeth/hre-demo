# UOW-01 Infrastructure Design Plan

## Unit: UOW-01 Foundation and Persistence Baseline

## Plan Checklist
- [x] Collect and validate answers to infrastructure questions
- [x] Map logical components to concrete infrastructure services
- [x] Define deployment architecture for UOW-01 runtime and stores
- [x] Generate `infrastructure-design.md`
- [x] Generate `deployment-architecture.md`

---

## Infrastructure Questions

### Q1: Deployment Environment
Where should UOW-01 be deployed for this phase?

A. Local Docker Compose only (dev-focused, no cloud deployment yet)  
B. AWS environment (dev/prod path aligned now)  
C. Azure environment (dev/prod path aligned now)  
D. GCP environment (dev/prod path aligned now)  
X. Other  

[Answer]: A

---

### Q2: Compute Infrastructure
What compute service should host the C# API container?

A. Docker Compose service (single host)  
B. Kubernetes deployment (managed cluster)  
C. Cloud container service (serverless containers)  
X. Other  

[Answer]: A

---

### Q3: Storage Infrastructure (JSON Stores)
Where should encrypted JSON store files live at runtime?

A. Mounted persistent volume attached to container runtime  
B. Object storage as primary store (application reads/writes objects directly)  
C. Host local filesystem only (ephemeral/non-persistent acceptable)  
X. Other  

[Answer]: A

---

### Q4: Backup and Restore Infrastructure
How should backup snapshots for quarantine/auto-restore be implemented?

A. Scheduled snapshot job writing encrypted backup archives to object storage  
B. Volume snapshots only (infrastructure-native)  
C. Manual backup procedures only for this phase  
X. Other  

[Answer]: A

---

### Q5: Messaging Infrastructure
For UOW-01 internal write queues and recovery orchestration, what is preferred?

A. In-process queues only (no external message broker)  
B. External message broker/service bus for queueing and recovery tasks  
C. Hybrid (in-process now, external broker planned and pre-modeled)  
X. Other  

[Answer]: A

---

### Q6: Networking Infrastructure
What ingress/network pattern should be used?

A. Direct service exposure via internal network only (no public ingress in UOW-01)  
B. Reverse proxy / load balancer in front of service  
C. API gateway in front of service  
X. Other  

[Answer]: A

---

### Q7: Monitoring Infrastructure
What observability stack should be defined for UOW-01?

A. Serilog to stdout + OpenTelemetry metrics/traces + centralized log sink  
B. Serilog to file only (local) for this phase  
C. Minimal logging only; observability deferred  
X. Other  

[Answer]: A

---

### Q8: Shared Infrastructure Strategy
Should UOW-01 infrastructure be isolated or shared with downstream units?

A. Shared base infrastructure across all UOWs (with clear namespace boundaries)  
B. Isolated infrastructure for UOW-01, then consolidate later  
C. Mixed: shared observability/network, isolated storage/compute  
X. Other  

[Answer]: A

---

### Q9: Environment Topology
How many environments should be designed now?

A. Dev only  
B. Dev + Staging  
C. Dev + Staging + Production  
X. Other  

[Answer]: A

---

### Q10: Release Control for Preview Runtime
The current runtime decision is .NET 10 preview. What infra-level release control do you want?

A. Block promotion to production environment when preview runtime detected  
B. Allow promotion with explicit approval note and risk warning  
C. Keep preview runtime only in dev/staging, force supported runtime in production manifests  
X. Other  

[Answer]: B

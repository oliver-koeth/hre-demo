# UOW-03 NFR Design Plan

## Unit: UOW-03 Governance and Evidence Domains

### Plan Checklist
- [x] Collect and validate answers to NFR design questions
- [x] Define resilience patterns for retention, legal-hold enforcement, and export flows
- [x] Define scalability/performance patterns for audit query and evidence export paths
- [x] Define security/compliance patterns for governance data handling
- [x] Define logical components and interaction boundaries
- [x] Generate `nfr-design-patterns.md`
- [x] Generate `logical-components.md`

---

## NFR Design Questions

### Q1: Retention Invocation Failure Handling Pattern
How should failed manual retention invocations be handled?

A. Immediate fail response + structured alert event + no partial success response  
B. Retry loop with exponential backoff before fail response  
C. Queue failed operation for deferred worker retry and return accepted  
X. Other  

[Answer]: A

---

### Q2: Idempotence Enforcement Pattern
How should retention idempotence be guaranteed for repeated evaluations on unchanged entities?

A. Deterministic decision function + persisted decision fingerprint per entity/rule/state  
B. Stateless re-evaluation only (no decision fingerprint persistence)  
C. Versioned lifecycle snapshots with idempotence token checks  
X. Other  

[Answer]: A

---

### Q3: Audit Query Performance Pattern
Which pattern should support 20 req/s security-event queries?

A. Time-windowed query filtering with bounded page size defaults  
B. Full-scan query each request (no paging constraints)  
C. Pre-aggregated query index cache refreshed periodically  
X. Other  

[Answer]: A

---

### Q4: Evidence Export Latency Pattern
How should P95 < 5s for 10k JSON record exports be designed?

A. Streaming JSON writer with chunked fetch pagination  
B. Materialize full payload in memory then serialize once  
C. Background export job with polling endpoint  
X. Other  

[Answer]: A

---

### Q5: Legal-Hold Fail-Closed Security Pattern
How should legal-hold blocking be enforced across governance operations?

A. Central policy guard service invoked before any data-output/mutation operation  
B. Per-endpoint inline checks only  
C. Data-store trigger level enforcement only  
X. Other  

[Answer]: A

---

### Q6: Incident Durability Pattern
How should single-store durable write be enforced for incident records?

A. Synchronous write + fsync confirmation before success response  
B. Synchronous write without durability confirmation  
C. Asynchronous buffered write with periodic flush  
X. Other  

[Answer]: A

---

### Q7: Backup Metadata Freshness Pattern
How should backup metadata become queryable within 30 seconds?

A. Immediate append to metadata store + lightweight in-memory read-through cache invalidation  
B. Periodic metadata ingestion poller (<=30s interval)  
C. Event-driven async projector with eventual consistency window  
X. Other  

[Answer]: A

---

### Q8: Alert Emission Pattern
How should immediate alerts for failed retention and blocked bypass attempts be emitted?

A. Structured security event + operational log alert hook in request path  
B. Batch alert emitter every minute  
C. Alerting only through dashboard counters  
X. Other  

[Answer]: A

---

### Q9: Evidence Retention/Archival Pattern
How should 180-day minimum evidence retention be modeled?

A. Retention-expiry metadata on records + manual archival/deletion command path  
B. Hard delete at fixed schedule with no archival metadata  
C. Tiered hot/cold storage split starting day 1  
X. Other  

[Answer]: A

---

### Q10: Logical Component Partitioning
How should UOW-03 governance runtime be partitioned?

A. `AuditQueryService`, `EvidenceService`, `DataSubjectService`, `RetentionService`, `IncidentService`, `BackupEvidenceService`, `LegalHoldPolicyGuard`, `AlertService`  
B. Single `GovernanceService` with internal modules only  
C. Hybrid: evidence+audit combined, lifecycle/incident separate services  
X. Other  

[Answer]: B

# UOW-01 NFR Design Plan

## Unit: UOW-01 Foundation and Persistence Baseline

## Plan Checklist
- [x] Collect and validate answers to NFR design questions
- [x] Define NFR design patterns for resilience, scalability, performance, and security
- [x] Define logical components and their interaction boundaries
- [x] Generate `nfr-design-patterns.md`
- [x] Generate `logical-components.md`

---

## NFR Design Questions

### Q1: Resilience Pattern for Store Access Failures
When store read/write fails because of transient file I/O issues, what resilience behavior should apply first?

A. Retry with bounded exponential backoff (max 3 attempts) before returning failure  
B. Fail immediately (no retry) to keep behavior deterministic and avoid duplicated work  
C. Retry once, then degrade to read-only mode for affected operations  
X. Other  

[Answer]: A

---

### Q2: Integrity Failure Handling
When HMAC verification or AES-GCM authentication fails for a store file, how should the service behave?

A. Fail closed immediately; block operations on that store until operator intervention  
B. Quarantine the file and automatically restore from latest verified backup snapshot  
C. Keep read operations but block writes until integrity is restored  
X. Other  

[Answer]: B

---

### Q3: Concurrency Pattern for Writes
What write-coordination pattern should the NFR design adopt for JSON stores inside one service instance?

A. Per-store async write queue + optimistic concurrency at record level  
B. Global process-wide mutex for all stores + optimistic concurrency  
C. File-lock-first design with optimistic concurrency only as fallback  
X. Other  

[Answer]: A

---

### Q4: Scalability Boundary for JSON Store Size
At what threshold should UOW-01 design trigger a scale strategy change (partitioning/rollover) for a single store file?

A. 25 MB per file  
B. 50 MB per file  
C. 100 MB per file  
X. Other  

[Answer]: B

---

### Q5: Performance Pattern for Read Latency
To protect P99 read latency target (<100ms), which primary optimization should be designed first?

A. In-memory index for key lookups (ID/username/email), still verifying integrity on load cycles  
B. Full-file scan on each query (simpler design, no in-memory index)  
C. Hybrid: cache only hot entity indexes with TTL invalidation  
X. Other  

[Answer]: A

---

### Q6: Performance Pattern for Write Latency
To protect P99 write latency target (<250ms), which write pattern should be used?

A. Batch flush window (e.g., 25–50ms) with atomic commit of grouped writes  
B. Immediate per-operation atomic write (no batching)  
C. Configurable mode: immediate by default, batching enabled for high-throughput environments  
X. Other  

[Answer]: B

---

### Q7: Security Pattern for Secrets Access
How should encryption/HMAC keys from mounted secret files be managed in memory?

A. Load once at startup into protected in-memory key provider; no per-request file reads  
B. Read secret files on every crypto operation (always latest key from disk)  
C. Load on startup with periodic timed reload (for rotation), with atomic key swap  
X. Other  

[Answer]: A

---

### Q8: Security Pattern for Audit Logging of Foundation Operations
Which events from UOW-01 internals should be mandatory security logs?

A. Integrity failures, migration events, concurrency conflicts, and key-loading failures  
B. Only integrity failures and key-loading failures  
C. All read/write operations including successful events  
X. Other  

[Answer]: A

---

### Q9: Logical Component Boundary for Crypto and Integrity
How should crypto and integrity logic be structured?

A. Separate `EncryptionService` and `IntegrityService`, orchestrated by `StorePersistenceService`  
B. Single combined `SecureStoreService` containing both crypto and integrity logic  
C. Crypto/integrity implemented directly inside each store repository for local control  
X. Other  

[Answer]: A

---

### Q10: Runtime Version Risk Control
Given .NET 10 preview was selected, what NFR design control should be enforced for release gating?

A. Mandatory CI gate that fails release pipelines unless runtime is switched to supported GA/LTS/STS  
B. Advisory warning only; team decides manually at release time  
C. Dual-track build (preview + LTS) throughout construction stages  
X. Other  

[Answer]: B

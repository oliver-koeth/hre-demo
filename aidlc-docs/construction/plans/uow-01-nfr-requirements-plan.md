# UOW-01 NFR Requirements Plan

## Unit: UOW-01 Foundation and Persistence Baseline

## Context Summary
Many NFR decisions are already locked in from INCEPTION:
- Language: **C#**, Runtime: **Docker**, Persistence: **JSON files**
- DR strategy: **Warm Standby** (RTO/RPO in minutes)
- CI/CD: **GitHub Actions** (Blue/Green deployment, rollback via swap-back)
- Change management: lightweight project-level process until org process replaces it
- At-rest encryption: **AES-256-GCM** (file-level), integrity: **HMAC-SHA256**
- Security: deny-by-default, Argon2id credential hashing, HMAC-signed stores

The questions below target only the **open tech stack choices** specific to UOW-01.

---

### Plan Checklist
- [x] Collect and validate answers to tech stack questions
- [x] Generate `nfr-requirements.md`
- [x] Generate `tech-stack-decisions.md`

---

## NFR Questions

### Q1: Target .NET Version
Which .NET version should the service target?

A. .NET 8 LTS (Recommended) — long-term support until November 2026; stable; widest ecosystem compatibility  
B. .NET 9 (Current / STS) — latest features; standard support only until May 2026  
C. .NET 10 (Preview) — not yet GA; not suitable for production  
X. Other  

[Answer]: C

---

### Q2: JSON Serialisation Library
Which library should handle JSON serialisation/deserialisation of store files?

A. `System.Text.Json` (built-in) — high performance, source-generator support, no extra dependency  
B. `Newtonsoft.Json` (Json.NET) — mature, extensive feature set; heavier but more forgiving  
X. Other  

[Answer]: A

---

### Q3: Logging Framework
Which logging framework should be used for structured logging across the service?

A. **Serilog** with console and file sinks — rich structured logging, widely used in .NET ecosystem  
B. `Microsoft.Extensions.Logging` (MEL) with no additional sink library — framework-native, minimal dependencies  
C. **NLog** — configuration-file-driven, mature, low overhead  
X. Other  

[Answer]: A

---

### Q4: Test Runner and Assertion Library
Which test runner and assertion library should the project use?

A. **xUnit** + **FluentAssertions** — parallel by default, idiomatic .NET, widely adopted  
B. **NUnit** + **FluentAssertions** — attribute-based, flexible setup/teardown  
C. **MSTest v3** — Microsoft-native, integrated with Visual Studio  
X. Other  

[Answer]: A

---

### Q5: Property-Based Testing Framework
Which PBT framework should be used? (PBT-09 mandates selection here.)

A. **FsCheck** (Recommended for C#/.NET) — integrates with xUnit/NUnit; shrinking; seed reproducibility; well-established  
B. **CsCheck** — newer; combines PBT with benchmarking; smaller community  
X. Other  

[Answer]: A

---

### Q6: Persistence Read P99 Latency Target
What is the acceptable P99 latency for a persistence read operation (HMAC verify + AES decrypt + JSON parse + query)?

A. < 50ms — reasonable target for local file I/O including all cryptographic overhead  
B. < 100ms — relaxed target; allows for cold-start, larger store files, and GC pauses  
C. < 200ms — permissive; suitable if persistence is a background concern only  
X. Other  

[Answer]: B

---

### Q7: Persistence Write P99 Latency Target
What is the acceptable P99 latency for a persistence write operation (serialise + AES encrypt + atomic write + HMAC sign)?

A. < 100ms — reasonable target including atomic rename + HMAC sidecar write  
B. < 250ms — relaxed; accommodates larger store files and OS-level flush  
C. < 500ms — permissive; acceptable if writes are infrequent relative to reads  
X. Other  

[Answer]: B

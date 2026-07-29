# DORA Compliance Documentation Rules

## Overview

These rules enforce creation and ongoing maintenance of a DORA compliance document for insurance-sector engineering delivery, scoped to the project lifecycle context actually available in this repository.

**Enforcement**: At each applicable AI-DLC stage, the model MUST verify compliance with these rules before presenting the stage completion message.

### Blocking DORA Finding Behavior

A **blocking DORA finding** means:
1. The finding MUST be listed in the stage completion message under a "DORA Findings" section with the DORA rule ID and description.
2. The stage MUST NOT present the "Continue to Next Stage" option until all blocking findings are resolved.
3. The model MUST present only the "Request Changes" option with a clear explanation of what needs to change.
4. The finding MUST be logged in `aidlc-docs/audit.md` with rule ID, description, and stage context.

If a DORA rule is not applicable to the current project or stage, mark it as **N/A** in the compliance summary (non-blocking).

### Default Enforcement

All rules in this document are **blocking** by default.

---

## Rule DORA-01: Mandatory Artifact Presence

**Rule**: The project MUST maintain a DORA compliance artifact at:

`aidlc-docs/compliance/dora-compliance.md`

If the file does not exist, bootstrap it from:

`./.aidlc-rule-details/extensions/compliance/dora/dora-compliance-template.md`

**Verification**:
- DORA document exists at the required path.
- Document uses template section structure or a strict superset.

---

## Rule DORA-02: Scope Boundary and Ownership Clarity

**Rule**: The document MUST explicitly state scope boundaries (for this repo: development and local integration testing) and MUST mark unsupported operational sections with the exact phrase:

`will be provided by operations`

**Verification**:
- Scope section clearly states lifecycle boundary and environment constraints.
- Each not-yet-evidenced operational section uses the exact required phrase.

---

## Rule DORA-03: Control-Oriented Structure

**Rule**: The DORA document MUST include the following control-oriented sections:
- Document control and scope
- Regulatory context and DORA mapping
- Compliance posture summary
- SDLC governance and approvals
- Secure change/release controls
- Vulnerability management
- Security testing evidence
- Resilience/performance testing evidence
- Incident response (or operations ownership marker)
- Business continuity/disaster recovery (or operations ownership marker)
- Monitoring and logging (or operations ownership marker)
- Third-party ICT risk management (or operations ownership marker)
- Data protection and retention (or operations ownership marker)
- Control mapping matrix
- Gap register and action plan
- Evidence index
- Integrity stamp block

**Verification**:
- All required section headings exist.
- Each section contains evidence, status, or required operations marker.

---

## Rule DORA-04: Evidence-Linked Claims Only

**Rule**: Compliance claims MUST be backed by concrete repository evidence (file path or workflow). Unsupported claims are not permitted.

**Verification**:
- Every implemented control statement references at least one evidence location.
- Evidence index includes direct paths to workflows/tests/docs used by the claim.
- No section claims production controls without evidence.

---

## Rule DORA-05: Freshness and Update Triggers

**Rule**: The DORA document MUST be refreshed whenever compliance-relevant artifacts change, including:
- `.github/workflows/**`
- `tests/**` (security/performance/integration controls)
- `aidlc-docs/**` traceability/build-and-test/security/performance docs
- `src/**` security or governance behavior affecting controls

**Verification**:
- DORA document timestamp and commit stamp reflect latest relevant change set.
- Control mapping and evidence index are updated for changed controls.

---

## Rule DORA-06: Mandatory Integrity Stamp Block

**Rule**: The DORA document MUST include an integrity stamp block with:
- Repository
- Branch
- Commit SHA (full 40-char)
- Document SHA-256 (exact file content)
- Generated At (UTC, ISO 8601)
- Generator Identity (workflow/job + run ID, or local generation context)

**Verification**:
- All stamp fields are present and non-empty.
- Commit SHA is 40 characters.
- SHA-256 format is 64 lowercase hex characters.
- Stamp fields align with current repository state at generation time.

---

## Rule DORA-07: Gap Register Discipline

**Rule**: Gaps MUST be explicit and tracked with owner, status, and target timeline. If unknown, placeholder values are permitted but cannot be omitted.

**Verification**:
- Gap table exists and includes: Gap ID, Description, Owner, Status, Target Date, Dependency.
- Open gaps are consistent with sections marked "will be provided by operations".

---

## Rule DORA-08: Bootstrap and Continuous Maintenance Mode

**Rule**: Initial post-implementation generation MUST be supported as a one-time bootstrap. Subsequent AI-DLC runs MUST maintain the artifact automatically when relevant evidence changes.

**Verification**:
- Initial doc creation path is documented.
- Ongoing update behavior is enforced by stage completion checks.

---

## Enforcement Integration

These rules are cross-cutting constraints that apply to the following AI-DLC stages:

| Stage | Applicable Rules | Enforcement |
|---|---|---|
| Requirements Analysis | DORA-01, DORA-02, DORA-03 | Ensure doc scope and structure requirements are planned |
| Workflow Planning | DORA-03, DORA-05, DORA-08 | Ensure DORA maintenance is included in stage plan |
| Code Generation (Planning) | DORA-04, DORA-05 | Require evidence update steps when control-relevant changes are planned |
| Code Generation (Generation) | DORA-04, DORA-05, DORA-06, DORA-07 | Update doc content + stamp + gap register with actual changes |
| Build and Test | DORA-04, DORA-05, DORA-06 | Refresh evidence index and stamp from latest verification artifacts |

At each applicable stage:
- Evaluate each DORA rule as compliant, non-compliant, or N/A.
- Include a "DORA Compliance" section in completion output.
- Any non-compliant applicable rule is blocking.

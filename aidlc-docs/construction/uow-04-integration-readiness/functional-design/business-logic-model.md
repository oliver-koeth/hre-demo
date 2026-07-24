# UOW-04 Business Logic Model

## Overview
UOW-04 executes an integration-readiness quality gate across UOW-01..UOW-03 before Build and Test.

## Logic Flow
1. **Collect unit readiness states**
   - Read unit completion states for UOW-01, UOW-02, and UOW-03.
   - Create one `IntegrationReadinessRecord` per unit.

2. **Validate cross-unit contract consistency**
   - Validate public API request/response contract consistency.
   - Validate shared primitive consistency (IDs, result/error contracts, correlation semantics).
   - Validate repository interface consistency across module boundaries.
   - Record each outcome as `ContractConsistencyCheck`.

3. **Build story-to-file traceability map**
   - For each story `US-01..US-12`, map to owning unit and evidence file list.
   - Mark entry as `Covered` when at least one file path is present; otherwise `Missing`.

4. **Evaluate unresolved integration items**
   - Collect unresolved findings from contract and traceability checks.
   - Convert each unresolved finding to an `UnresolvedIntegrationItem` with blocking impact.
   - If any item remains open, gate status is `Fail`.

5. **Validate error contract consistency**
   - Check that all API surfaces expose RFC7807-compatible error payloads containing `errorCode` and `correlationId`.
   - Any missing field is a blocking unresolved item.

6. **Validate runtime topology minimum**
   - Verify presence of `docker-compose.yml`.
   - Verify presence of `config/policy.template.json`.
   - Record result in `RuntimeReadinessRecord`.

7. **Compute quality-gate decision**
   - Set `allUnitsImplemented` from step 1.
   - Set `noBlockingUnresolvedItems` from step 4.
   - Set `errorContractConsistent` from step 5.
   - If all required booleans are true, set checklist `status=Pass`; otherwise `status=Fail`.
   - Persist one short summary note for each decision.

## Decision Semantics
- **Pass**: Build and Test may start.
- **Fail**: Build and Test is blocked until all unresolved blocking items are resolved.

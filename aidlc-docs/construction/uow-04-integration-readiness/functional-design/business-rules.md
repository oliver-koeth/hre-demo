# UOW-04 Business Rules

## BR-01: Unit-Level Readiness Scope
Integration readiness records store unit-level completion status and note text only.

## BR-02: Mandatory Contract Validation Surfaces
The gate must validate consistency for:
1. Public API request/response contracts.
2. Shared domain primitives.
3. Repository interfaces.

## BR-03: Traceability Evidence Granularity
Each story (`US-01..US-12`) must map to its owning unit and at least one implementation file path.

## BR-04: Blocking Policy for Unresolved Findings
Any unresolved integration item is blocking and prevents gate passage.

## BR-05: Build-and-Test Entry Criterion
Gate passes only when all units are implemented and no unresolved blocking items exist.

## BR-06: Error Contract Consistency
All unit API error responses must consistently provide RFC7807 structure with `errorCode` and `correlationId`.

## BR-07: Runtime Topology Minimal Validation
Gate must confirm both `docker-compose.yml` and `config/policy.template.json` exist.

## BR-08: Checklist Ownership Model
A single owner is assigned for the complete integration checklist.

## BR-09: Completion Evidence Format
Every integration check stores status plus a short note (no mandatory link or sign-off timestamp in V1).

## BR-10: Property-Based Testing Scope
UOW-04 introduces no new PBT suites and reuses prior unit-level PBT evidence.

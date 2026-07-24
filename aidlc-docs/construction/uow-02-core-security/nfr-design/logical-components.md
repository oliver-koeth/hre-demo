# Logical Components — UOW-02 NFR Design

## Deployment Shape
UOW-02 uses a single deployable runtime: **AuthControlPlaneService**.
Within it, the following internal modules are logical components with strict interfaces.

## LC-U02-01 AuthModule
- Handles login request processing and credential verification orchestration.
- Owns failed-attempt tracking and lockout threshold updates.

## LC-U02-02 TokenValidationModule
- Validates JWT signature, expiry, issuer, audience.
- Uses short-TTL cache for user status and token-version checks.

## LC-U02-03 AuthorizationModule
- Resolves effective permission grants from role assignments.
- Evaluates `resource:action` decisions with default-deny behavior.

## LC-U02-04 ApprovalModule
- Manages governed role-permission approval lifecycle.
- Performs synchronous persistence with bounded retries.
- Enforces non-self-approval and terminal-state constraints.

## LC-U02-05 RateLimitModule
- Tracks per-IP and per-account fixed-window counters.
- Applies hard deny when thresholds are breached.

## LC-U02-06 MfaIntegrationModule
- Adapts to external MFA provider using synchronous verification calls.
- Denies sensitive operation when verification is missing or fails.

## LC-U02-07 DisableConsistencyModule
- Coordinates disable + token version increment in a transaction-style sequence.
- Guarantees strong consistency before success response.

## LC-U02-08 ErrorMappingMiddleware
- Global middleware mapping domain failures to RFC7807 responses.
- Injects `errorCode` and `correlationId` consistently.

## LC-U02-09 SecurityAlertModule
- Maintains rolling lockout windows (5 events / 10 minutes).
- Emits immediate operational alerts and structured security events.

## LC-U02-10 RetentionArchivalModule
- Keeps hot security event retention >= 90 days.
- Triggers periodic archival snapshot workflow.

---

## Interaction Flow
1. Request enters AuthControlPlaneService.
2. RateLimitModule pre-check applies.
3. AuthModule or TokenValidationModule processes request.
4. If admin-sensitive action: MfaIntegrationModule verification required.
5. AuthorizationModule evaluates required permission.
6. If governed permission change: ApprovalModule handles SoD flow.
7. ErrorMappingMiddleware standardizes failures.
8. SecurityAlertModule and RetentionArchivalModule persist/alert as needed.

---

## Responsibility Matrix
| Concern | Logical Component |
|---|---|
| Login and lockout | AuthModule |
| Token checks + cache | TokenValidationModule |
| Permission decisions | AuthorizationModule |
| SoD approval workflow | ApprovalModule |
| Per-IP/account throttling | RateLimitModule |
| External MFA verification | MfaIntegrationModule |
| Strong disable consistency | DisableConsistencyModule |
| RFC7807 mapping | ErrorMappingMiddleware |
| Lockout alerting | SecurityAlertModule |
| Event retention/archival | RetentionArchivalModule |

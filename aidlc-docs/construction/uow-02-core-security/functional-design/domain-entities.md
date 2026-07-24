# Domain Entities — UOW-02 Core Auth and Authorization Control Plane

## Scope
This unit implements US-01..US-06 using UOW-01 foundations. It introduces runtime entities for authentication sessions, token lifecycle, authorization decisions, and SoD approval workflow for governed admin changes.

## Applied Decisions
| Topic | Decision |
|---|---|
| Login identifier | Email only |
| Hash verification | Accept Argon2id + BCrypt; new/rotated credentials use Argon2id |
| Failed login policy | Hard lockout after threshold, unlock after configured duration |
| Token checks | Signature + expiry + issuer + audience + account status + token version |
| Step-up control | MFA challenge for every sensitive admin operation |
| Authorization evaluation | Allow if any assigned role grants permission; default deny |
| SoD governed scope | Role-permission assignment/revocation only |
| Approval timeout | No auto-timeout; pending until explicit decision |
| User lifecycle | Disable only in V1; no delete; immediate access termination |
| Permission key model | `resource:action` |
| API error model | Domain `Result` -> RFC7807 Problem Details with correlation ID |

---

## Runtime Entities

## AuthAttemptState
Tracks lockout and failed-attempt behavior by user email.
```
AuthAttemptState {
  EmailNormalized      : string
  FailedAttempts       : int
  LastFailureAt        : DateTimeOffset?
  LockedUntil          : DateTimeOffset?
  LastSuccessAt        : DateTimeOffset?
  Version              : int
}
```

## AuthSession
Represents authenticated context for active token/session checks.
```
AuthSession {
  SessionId            : Guid
  UserId               : Guid
  TokenVersionSnapshot : int
  IssuedAt             : DateTimeOffset
  ExpiresAt            : DateTimeOffset
  IsPrivileged         : bool
  RevokedAt            : DateTimeOffset?
  RevokeReason         : string?
}
```

## AccessTokenClaimsModel
Server-side normalized claim model after JWT parse/validation.
```
AccessTokenClaimsModel {
  SubjectUserId        : Guid
  SessionId            : Guid
  TokenVersion         : int
  Issuer               : string
  Audience             : string
  IssuedAt             : DateTimeOffset
  ExpiresAt            : DateTimeOffset
  PermissionKeys       : string[]   // e.g. users:read
  IsPrivileged         : bool
}
```

## AuthorizationRequestModel
```
AuthorizationRequestModel {
  UserId               : Guid
  Resource             : string
  Action               : string
  CorrelationId        : Guid
}
```

## AuthorizationDecisionModel
```
AuthorizationDecisionModel {
  Allowed              : bool
  ReasonCode           : string        // e.g. PERMISSION_MATCHED, DEFAULT_DENY
  PermissionEvaluated  : string        // resource:action
  CorrelationId        : Guid
}
```

## ApprovalTicket
SoD ticket for governed role-permission changes only.
```
ApprovalTicket {
  TicketId             : Guid
  ChangeType           : string        // ROLE_PERMISSION_ASSIGN | ROLE_PERMISSION_REVOKE
  RoleId               : Guid
  PermissionId         : Guid
  RequestedByUserId    : Guid
  RequestedAt          : DateTimeOffset
  Status               : ApprovalStatus
  ApprovedByUserId     : Guid?
  ApprovedAt           : DateTimeOffset?
  RejectedByUserId     : Guid?
  RejectedAt           : DateTimeOffset?
  RejectionReason      : string?
  CorrelationId        : Guid
  Version              : int
}

ApprovalStatus = Pending | Approved | Rejected | Applied
```

## StepUpChallenge
```
StepUpChallenge {
  ChallengeId          : Guid
  UserId               : Guid
  SessionId            : Guid
  OperationKey         : string        // target sensitive admin operation
  IssuedAt             : DateTimeOffset
  ExpiresAt            : DateTimeOffset
  CompletedAt          : DateTimeOffset?
  Status               : StepUpStatus
}

StepUpStatus = Pending | Satisfied | Failed | Expired
```

---

## Derived Keys and Value Objects

- **PermissionKey**: `${resource}:${action}` (normalized lowercase).
- **LoginIdentity**: normalized email (trim + lowercase).
- **ProblemErrorBody** (API output):
  - `type`, `title`, `status`, `detail`, `errorCode`, `correlationId`.

---

## Testable Properties (PBT-01)

| ID | Category | Property |
|---|---|---|
| PBT-U02-01 | Invariant | Authorization allows only when at least one active role grants exact `resource:action`; else denies |
| PBT-U02-02 | Invariant | Email normalization is idempotent: normalize(normalize(x)) = normalize(x) |
| PBT-U02-03 | Invariant | Lockout state transitions never produce negative failed-attempt count |
| PBT-U02-04 | Round-trip | JWT claims parse/normalize round-trip preserves required claim set (subject/session/tokenVersion/exp/iss/aud) |
| PBT-U02-05 | Idempotence | Disabling an already-disabled user leaves access state unchanged after first disable |
| PBT-U02-06 | Invariant | Approval ticket cannot transition from Rejected/Applied back to Pending |
| PBT-U02-07 | Invariant | MFA step-up required operations never execute when challenge status != Satisfied |


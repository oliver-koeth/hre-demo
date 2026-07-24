# Business Logic Model — UOW-02 Core Auth and Authorization Control Plane

## Story Coverage
- **US-01** Credential Authentication
- **US-02** Server-Side Token Validation
- **US-03** Privileged Session Controls
- **US-04** User Lifecycle Administration
- **US-05** Role and Permission Catalog Management
- **US-06** Authorization Enforcement on Protected Endpoints

---

## 1. Authentication Flow (US-01)

1. Receive login request with email + password.
2. Normalize email and load user + active credential.
3. Check lockout state.
4. Verify password hash (Argon2id or BCrypt).
5. On failure: increment attempts, possibly lock, emit audit event, return auth failure.
6. On success:
   - reset failure counters,
   - verify account is active,
   - create session,
   - issue JWT,
   - emit success event,
   - return token response.

Decision table:

| Condition | Outcome |
|---|---|
| Email not found | Generic auth failure |
| Account locked | Auth failure + lock reason |
| Password mismatch | Auth failure, increment attempts |
| Account inactive/disabled | Auth failure |
| Password valid + account active | Issue token |

---

## 2. Protected Request Validation (US-02)

1. Extract bearer token.
2. Validate JWT signature.
3. Validate expiry, issuer, audience.
4. Resolve subject user and ensure account still active.
5. Compare token version claim with current user token version.
6. If all pass: attach auth context to request.
7. Else: deny request and emit token rejection event.

---

## 3. Privileged Session Step-Up (US-03)

Sensitive admin operation pipeline:

1. Detect operation as sensitive.
2. Require MFA challenge creation or existing valid challenge.
3. Verify challenge result is `Satisfied`.
4. If challenge not satisfied: deny operation.
5. If satisfied: continue to authorization + business action.

---

## 4. Authorization Enforcement (US-06)

1. Build required permission key: `${resource}:${action}`.
2. Resolve active user role assignments.
3. Resolve active role-permission assignments.
4. Check if any assignment grants exact key.
5. Return allow/deny decision with reason code.

Pseudo-evaluation:
```
allowed = exists activeRole in userRoles
           where exists activeGrant in rolePermissionGrants
             where activeGrant.roleId == activeRole.roleId
               and activeGrant.permissionKey == requiredKey
if allowed then PERMISSION_MATCHED else DEFAULT_DENY
```

---

## 5. Admin User Lifecycle (US-04)

Supported in V1:
- create user
- update user
- disable user (no hard delete)

Disable operation model:
1. authorize caller for `users:disable`
2. apply disable state
3. increment token version for immediate access termination
4. emit admin change event

---

## 6. Role/Permission Management + SoD (US-05)

### Non-governed actions
- role CRUD
- permission CRUD

### Governed actions (SoD)
- role-permission assign
- role-permission revoke

Governed mutation flow:
1. request change -> create `ApprovalTicket(Pending)`
2. second actor reviews -> `Approved` or `Rejected`
3. if approved -> apply mutation -> ticket `Applied`
4. emit approval and mutation audit events

Constraints:
- requestor cannot approve own ticket
- rejected/applied are terminal states
- pending remains until explicit decision

---

## 7. API Contract Mapping

Domain `Result` maps to HTTP:

| Domain Error | HTTP Status | errorCode |
|---|---|---|
| ValidationFailed | 400 | VALIDATION_FAILED |
| Unauthorized | 401 | UNAUTHORIZED |
| Forbidden | 403 | FORBIDDEN |
| NotFound | 404 | NOT_FOUND |
| Conflict | 409 | CONFLICT |
| IntegrityViolation/Internal | 500 | INTERNAL_ERROR |

Each error response includes `correlationId`.

---

## 8. PBT-Focused Behaviors for Downstream Code Generation

- Permission resolution invariant: allow iff at least one exact active grant exists.
- Token validation invariants: all mandatory checks are conjunctive; any single failure denies.
- Disable-user idempotence: second disable preserves disabled state and does not re-enable access.
- Approval workflow state invariants: no invalid backward transitions.
- Email normalization idempotence.


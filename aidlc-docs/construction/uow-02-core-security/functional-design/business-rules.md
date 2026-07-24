# Business Rules — UOW-02 Core Auth and Authorization Control Plane

## BR-U02-01 Login Identity Handling
- Login input accepts **email only**.
- Email is normalized (trim + lowercase) before lookup.
- Unknown email returns generic authentication failure (no user enumeration).

## BR-U02-02 Credential Verification Algorithm
- Verification accepts credentials hashed with Argon2id or BCrypt.
- New credential creation/rotation emits Argon2id only.
- Unsupported algorithm -> authentication failure with security event.

## BR-U02-03 Failed Login and Lockout
- On failed authentication, increment failure count.
- If failures reach configured threshold, set lockout until `now + lockoutDuration`.
- Locked accounts reject login attempts until lockout window ends.
- Successful authentication resets failure count and lockout state.

## BR-U02-04 Token Issuance
- On successful login and account allowed state:
  - issue JWT with subject user ID, session ID, token version, issuer, audience, expiry.
- Privileged sessions use shorter token TTL policy.
- Token issuance writes security audit event synchronously.

## BR-U02-05 Token Validation
- Every protected request validates:
  1. JWT signature
  2. expiry
  3. issuer
  4. audience
  5. account status still active
  6. token version equals current user token version
- Any failed check => deny request (default fail-closed).

## BR-U02-06 Immediate Access Termination
- User disable operation increments user token version and marks user inactive.
- All existing tokens with old token version become invalid immediately.
- V1 supports disable only (no user hard delete in this unit).

## BR-U02-07 Authorization Evaluation
- Required permission key is `${resource}:${action}`.
- Resolve active role assignments and active role-permission assignments.
- Authorization is allowed when **any** active assignment grants exact permission key.
- If no match, return deny with reason `DEFAULT_DENY`.

## BR-U02-08 Privileged Step-Up
- Sensitive admin operations require MFA challenge completion per operation execution.
- If challenge missing/expired/failed, operation is denied.
- MFA challenge status must be `Satisfied` at execution time.

## BR-U02-09 SoD Governance Scope
- Two-person approval applies only to:
  - role-permission assignment
  - role-permission revocation
- Non-governed admin operations execute without approval ticket.

## BR-U02-10 Approval Workflow Rules
- Approval ticket lifecycle:
  - Pending -> Approved/Rejected
  - Approved -> Applied
- Pending tickets remain pending until explicit decision (no auto-timeout).
- Requestor cannot self-approve their own ticket.
- Applied/Rejected tickets are terminal.

## BR-U02-11 API Error Contract
- Domain failures map to RFC7807 Problem Details.
- Response includes stable `errorCode` and `correlationId`.
- Internal details (keys, stack traces, sensitive fields) are never exposed.

## BR-U02-12 Security Event Capture
- Mandatory events captured synchronously for:
  - login success/failure/lockout
  - token issued/rejected
  - privileged operation step-up failure
  - role-permission approval requested/approved/rejected/applied


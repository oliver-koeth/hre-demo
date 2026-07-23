# Requirements Verification Questions

Please answer every question by filling in the letter after each `[Answer]:` tag.

## Question 1
Which authentication mechanism should the backend use as the primary flow?

A) Username/password with JWT access token

B) API key authentication

C) Mutual TLS client certificate authentication

D) OAuth2/OpenID Connect token validation from external identity provider

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 2
Where should user identities and credentials be managed?

A) Inside this service using local user records in JSON persistence

B) Inside this service but credentials only (minimal profile fields)

C) External identity system only; this service validates externally issued tokens

D) Hybrid: local service accounts plus external human user identities

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 3
What authorization model is required for this first backend version?

A) Authentication only (no role/permission checks yet)

B) Role-based access control with a fixed role set in configuration

C) Permission-based model with endpoint-level permissions

D) Tenant-aware role model for future multi-company access control

X) Other (please describe after [Answer]: tag below)

[Answer]: B, C, roles and permissions shall be configurable by admin, one role comprises multiple permissions

## Question 4
How should token lifecycle be handled?

A) Short-lived access token only; re-login required on expiry

B) Access + refresh token pair with rotation

C) Access token + server-side revocation list persisted in JSON

D) Stateless JWT with no revocation in first version

X) Other (please describe after [Answer]: tag below)

[Answer]: D

## Question 5
Which API surface should be included now?

A) `POST /auth/login` only

B) `POST /auth/login` and `POST /auth/validate`

C) `POST /auth/login`, `POST /auth/refresh`, and `POST /auth/logout`

D) Full auth admin APIs (user CRUD + password reset + role assignment)

X) Other (please describe after [Answer]: tag below)

[Answer]: D, for all objects mentioned in Q3

## Question 6
How should secrets and crypto material be provided in Docker runtime?

A) Environment variables only

B) Mounted secret files (Docker secrets or bind-mounted files)

C) Environment variables for dev, secret files for non-dev

D) External secret manager integration from day one

X) Other (please describe after [Answer]: tag below)

[Answer]:B

## Question 7
What level of audit logging is required for authentication events?

A) Minimal logs (success/failure with timestamp and user identifier)

B) Detailed security audit trail (event type, actor, source IP, reason, correlation ID)

C) Minimal now, detailed logging planned for later

D) No auth audit logging in first version

X) Other (please describe after [Answer]: tag below)

[Answer]: B

## Question 8
What non-functional priority should guide this backend MVP?

A) Security first, even with slower delivery

B) Delivery speed first for prototype validation

C) Balanced security and delivery speed

D) Performance and scalability first

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 9
What JSON persistence behavior is expected?

A) Single file store with in-memory cache and periodic flush

B) Direct read/write to JSON file on each auth-relevant operation

C) Append-only event log JSON plus derived snapshots

D) JSON persistence only for development, with clear path to database later

X) Other (please describe after [Answer]: tag below)

[Answer]: B

## Question 10
Should security extension rules be enforced for this project?

A) Yes — enforce all SECURITY rules as blocking constraints (recommended for production-grade applications)

B) No — skip all SECURITY rules (suitable for PoCs, prototypes, and experimental projects)

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 11
Should the resiliency baseline be applied to this project?

A) Yes — apply the resiliency baseline as directional best practices and design-time guidance (recommended for business-critical workloads, as an informed starting point that you can validate and harden before go-live)

B) No — skip the resiliency baseline (suitable for PoCs, prototypes, and experimental projects where rapid iteration matters more than reliability)

X) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 12
Should property-based testing (PBT) rules be enforced for this project?

A) Yes — enforce all PBT rules as blocking constraints (recommended for projects with business logic, data transformations, serialization, or stateful components)

B) Partial — enforce PBT rules only for pure functions and serialization round-trips (suitable for projects with limited algorithmic complexity)

C) No — skip all PBT rules (suitable for simple CRUD applications, UI-only projects, or thin integration layers with no significant business logic)

X) Other (please describe after [Answer]: tag below)

[Answer]: A

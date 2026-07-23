# Requirements Clarification Questions

I detected unresolved requirements required by enabled extensions and one ambiguity in prior answers. Please answer all questions.

## Question 1
Your answer to Question 3 combined RBAC and permission-based control. Which authorization source of truth should this service implement first?

A) Role-based access control only, with fixed permissions hard-mapped to each role in configuration

B) Permission-based control only, with users assigned permissions directly

C) Hybrid RBAC + permissions, where roles are configurable and each role contains a configurable permission set

D) Tenant-aware hybrid model from day one (roles and permissions scoped by tenant/company)

X) Other (please describe after [Answer]: tag below)

[Answer]: C

## Question 2
What are your Recovery Time Objective (RTO) and Recovery Point Objective (RPO) goals? These determine the disaster recovery strategy and redundancy level.

A) RPO/RTO: Hours — Backup & Restore strategy

B) RPO/RTO: Tens of minutes — Pilot Light strategy

C) RPO/RTO: Minutes — Warm Standby strategy

D) RPO/RTO: Near real-time — Multi-site Active/Active strategy

E) N/A — Single-region deployment is acceptable, no cross-region DR needed

X) Other (please describe after [Answer]: tag below)

[Answer]: C

## Question 3
How should production changes for this workload be governed?

A) Use existing organizational change management process (please specify process/tool after [Answer]:)

B) No formal process exists; define a lightweight process for this project

C) N/A — exempt from formal change management (please provide rationale after [Answer]:)

X) Other (please describe after [Answer]: tag below)

[Answer]: B

## Question 4
What CI/CD tooling and deployment process should this workload use?

A) Use existing CI/CD pipeline (please specify tool after [Answer]:)

B) No pipeline exists; propose one for this project

X) Other (please describe after [Answer]: tag below)

[Answer]: GitHub Actions (git repo not yet established currently only working in local directory, we need to do so, gh CLI is installed)

## Question 5
How should a failed production deployment be rolled back?

A) Redeploy previous IaC/artifact version

B) Blue/green swap back

C) Canary auto-rollback on health/metric regression

D) Database-aware rollback required (explicit schema/data reversal strategy)

E) Use existing organizational rollback procedure (please specify after [Answer]:)

X) Other (please describe after [Answer]: tag below)

[Answer]: B

## Question 6
What deployment strategy is acceptable for this workload's risk profile?

A) Direct / in-place

B) Rolling

C) Blue/green

D) Canary

X) Other (please describe after [Answer]: tag below)

[Answer]: C

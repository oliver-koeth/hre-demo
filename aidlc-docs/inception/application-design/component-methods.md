# Component Methods

## Auth API Component
- `Task<LoginResponse> LoginAsync(LoginRequest request, RequestContext context)`
- `Task<TokenValidationResponse> ValidateTokenAsync(TokenValidationRequest request, RequestContext context)`

## Admin API Component
- `Task<UserDto> CreateUserAsync(CreateUserRequest request, AdminContext context)`
- `Task<UserDto> UpdateUserAsync(UpdateUserRequest request, AdminContext context)`
- `Task DisableUserAsync(DisableUserRequest request, AdminContext context)`
- `Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, AdminContext context)`
- `Task<PermissionDto> CreatePermissionAsync(CreatePermissionRequest request, AdminContext context)`
- `Task AssignRolePermissionAsync(AssignRolePermissionRequest request, AdminContext context)`

## Auth Domain Component
- `Task<AuthResult> AuthenticateCredentialsAsync(string username, string password, RequestContext context)`
- `Task<JwtToken> IssueAccessTokenAsync(AuthSubject subject, TokenPolicy policy)`
- `Task<TokenValidationResult> ValidateAccessTokenAsync(string token, RequestContext context)`
- `Task<bool> IsAccountAllowedAsync(Guid userId, RequestContext context)`

## Authorization Domain Component
- `Task<PermissionSet> ResolveEffectivePermissionsAsync(Guid userId)`
- `Task<AuthorizationDecision> AuthorizeAsync(AuthorizationRequest request)`
- `Task<RolePolicyResult> ValidateRolePermissionMutationAsync(RolePermissionChangeRequest request)`

## Approval Workflow Component
- `Task<ApprovalTicket> StartApprovalAsync(ApprovalCommand command, AdminContext initiator)`
- `Task<ApprovalDecisionResult> RecordApprovalDecisionAsync(ApprovalDecision decision, AdminContext approver)`
- `Task<bool> IsApprovedAsync(string approvalTicketId)`

## Audit and Evidence Component
- `Task RecordSecurityEventAsync(SecurityEvent eventData)`
- `Task RecordAdminChangeAsync(AdminChangeEvent eventData)`
- `Task<EvidencePackage> ExportEvidenceAsync(EvidenceQuery query, ReviewerContext context)`
- `Task<ControlMappingResult> MapEvidenceToControlIdsAsync(ControlMappingRequest request)`

## Privacy Governance Component
- `Task<DataSubjectRecord> RetrieveDataSubjectDataAsync(DataSubjectQuery query, ReviewerContext context)`
- `Task<DataCorrectionResult> CorrectDataAsync(DataCorrectionRequest request, ReviewerContext context)`
- `Task<DataRestrictionResult> RestrictProcessingAsync(DataRestrictionRequest request, ReviewerContext context)`
- `Task<DataExportResult> ExportDataAsync(DataExportRequest request, ReviewerContext context)`
- `Task<DataErasureResult> EraseDataAsync(DataErasureRequest request, ReviewerContext context)`
- `Task<ProcessingMetadataExport> ExportProcessingMetadataAsync(ProcessingMetadataQuery query, ReviewerContext context)`

## Incident and Continuity Component
- `Task<IncidentRecord> RecordIncidentAsync(IncidentRecordRequest request, OpsContext context)`
- `Task RecordBreachNotificationEvidenceAsync(BreachEvidenceRequest request, OpsContext context)`
- `Task RecordBackupExecutionAsync(BackupExecutionRecord request)`
- `Task RecordRestoreExecutionAsync(RestoreExecutionRecord request)`

## Persistence Gateway Components
- `Task<T?> GetAsync<T>(StoreQuery query)`
- `Task SaveAsync<T>(StoreCommand<T> command)`
- `Task<IReadOnlyList<T>> SearchAsync<T>(StoreSearchQuery query)`
- `Task<StoreIntegrityResult> VerifyIntegrityAsync(StoreIntegrityCheckRequest request)`

> Detailed business rules and edge-case behavior will be specified in per-unit Functional Design.

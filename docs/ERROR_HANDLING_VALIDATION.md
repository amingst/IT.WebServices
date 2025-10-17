# Error Handling and Validation Plan (Consolidated)

This document consolidates the implementation plan and the inventory of existing response error enums into a single place.

Order of work:
1) Protos
2) C# Services
3) C# Controllers

Non‑goals: No SQL schema changes and no changes to repository/data interfaces.

## Goals
- Standardize errors using gRPC status codes + google.rpc detail types.
- Add schema‑level validation to all request/response protos (upstream only if/when proto sources are available here).
- Keep backward compatibility; version only when necessary.

## Phase 1 — Protos
- Repo reality: The source `.proto` files are not present in this repository under `Fragments/Protos`. We will not change proto schemas here. Instead, we will:
  - Inventory existing error enums from generated artifacts and treat them as the canonical error taxonomy for now.
  - Map those enums to gRPC status codes and google.rpc details at the C# service layer.
  - If/when upstream proto sources are available, optionally add PGV annotations and error‑detail documentation to protos in a subsequent upstream PR.
- Import (upstream) `google/rpc/status.proto` and detail types (`error_details.proto`).
- Adopt `protoc‑gen‑validate` (PGV) annotations for field constraints (upstream only; no schema edits in this repo).
- Mapping (canonical):
  - INVALID_ARGUMENT → field validation; attach `BadRequest.field_violations`.
  - FAILED_PRECONDITION → business rule errors; attach `PreconditionFailure`.
  - NOT_FOUND / ALREADY_EXISTS → attach `ResourceInfo`.
  - PERMISSION_DENIED / UNAUTHENTICATED.
  - OUT_OF_RANGE (bounds), ABORTED (concurrency), UNAVAILABLE (downstream, may add `RetryInfo`), INTERNAL (unknown).
- Validation patterns (examples): IDs non‑empty/positive, name length bounds, enums with `UNKNOWN = 0` and `defined_only`, pagination caps, `start <= end`, non‑empty repeateds, and oneof exactly one set.
- Responses stay data‑only (no `error` fields). Partial failures modeled explicitly. For existing response `Error` enum fields (e.g., `*ResponseErrorType`), keep them unchanged.

## Phase 2 — C# Services

Primary approach: JSON‑driven validation that sets existing proto error enums (no proto annotations required).

- Validate requests at the service boundary using JSON rules loaded at runtime; do this before any repository call.
- On first matching rule violation, set the response `Error` enum (e.g., `ModifyResponseErrorType`, `ChangeOwnPasswordResponseErrorType`) to the configured value and return immediately. Do not alter repository/data interfaces.
- Optional: also raise `RpcException` with `INVALID_ARGUMENT` + `BadRequest` details to enrich capable clients; not required if you prefer enum‑only for compatibility.
- Add negative tests per RPC for invalid fields/business rules; log rule id and reason (no PII).

JSON rules (concise spec)
- Location: `configs/validation/{domain}/{service}/{rpc}.json` (e.g., `configs/validation/Authentication/UserService/ChangeOwnPassword.json`).
- Shape (per RPC):
  - `version`: string (e.g., "1")
  - `service`: string (optional, for reference)
  - `rpc`: string
  - `mode`: `first_error` | `collect_all` (default `first_error`)
  - `rules`: array of rule objects:
    - `id`: stable string (for logs/metrics)
    - `field`: proto field path (e.g., `user_name`, `profile_image`, supports nested and repeated indexes if needed)
    - `when`: optional predicate on other fields (simple comparisons)
    - `checks`: object with supported operators
      - strings: `required`, `minLen`, `maxLen`, `regex`
      - numbers: `min`, `max`, `gt`, `gte`, `lt`, `lte`
      - enums: `in`, `notIn`, `definedOnly`
      - repeated: `minItems`, `maxItems`, `unique`
      - cross‑field: `eq`, `ne`, `ltField`, `lteField`, `gtField`, `gteField`
    - `message`: human text for logs
    - `errorEnum`: fully‑qualified enum type as generated (e.g., `IT.WebServices.Fragments.Settings.ModifyResponseErrorType`)
    - `errorValue`: enum value name to return (e.g., `BadFormat`, `InvalidPassword`)
    - `stop`: bool (override `mode` to stop on this violation)

Example JSON (ChangeOwnPassword)
```
{
  "version": "1",
  "rpc": "ChangeOwnPassword",
  "mode": "first_error",
  "rules": [
    { "id": "old_password_required", "field": "old_password", "checks": { "required": true, "minLen": 8 }, "message": "Old password required", "errorEnum": "IT.WebServices.Fragments.Authentication.ChangeOwnPasswordResponse.ChangeOwnPasswordResponseErrorType", "errorValue": "BadOldPassword", "stop": true },
    { "id": "new_password_strength", "field": "new_password", "checks": { "minLen": 12, "regex": "(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])" }, "message": "Weak password", "errorEnum": "IT.WebServices.Fragments.Authentication.ChangeOwnPasswordResponse.ChangeOwnPasswordResponseErrorType", "errorValue": "WeakPassword" }
  ]
}
```

Runtime behavior
- Load rules at startup and watch for changes; cache per RPC.
- Evaluate rules against incoming request via reflection or generated accessors.
- On violation:
  - Set `response.Error = <errorEnum.errorValue>` and return the response.
  - Optional: map to `RpcException` with `INVALID_ARGUMENT` + `BadRequest` details.

Optional alternatives (later)
- protovalidate + protovalidate‑net or ProtoValidate NuGet can be adopted upstream; keep JSON as the override/customization layer if needed.

### C# Validation Tooling (Optional)
- If you later want schema‑embedded rules, consider protovalidate + protovalidate‑net (CEL) or ProtoValidate (PGV‑style). Keep JSON as the customization layer.

## Phase 3 — C# Controllers
- For HTTP inputs, mirror proto constraints with ASP.NET Core model validation; return RFC7807 `ProblemDetails` with field errors.
- Translate `RpcException` to HTTP responses:
  - INVALID_ARGUMENT → 400 (include field errors if `BadRequest`).
  - FAILED_PRECONDITION → 412; NOT_FOUND → 404; ALREADY_EXISTS → 409.
  - PERMISSION_DENIED/UNAUTHENTICATED → 403/401.
  - UNAVAILABLE → 503 (+ optional Retry‑After).
  - INTERNAL → 500.
- Preserve correlation IDs; expose non‑PII error metadata in headers if useful.

## Cross‑Cutting
- Inventory and rollout order:
  - Authorization/Payment (Manual, Fortis, Paypal, Stripe, Combined)
  - Authentication/Services
  - Content/CMS and Content/Stats
  - Settings/Services
  - Notification/Services
  - Authorization/Events
- Codegen: regenerate TS (`*_pb.ts`, `*_connect.ts`) and C# after proto updates (upstream); pin generator versions in CI.
- CI: lint protos (upstream), run PGV, run service negative tests, optional contract tests asserting error shapes.
- Observability: dashboards for INVALID_ARGUMENT, FAILED_PRECONDITION, NOT_FOUND, INTERNAL per method.

## Minimal Examples
Proto (PGV) — upstream only:
```
message UpdateSettingRequest {
  string setting_id = 1 [(validate.rules).string = {min_len: 1, max_len: 64}];
  string value      = 2 [(validate.rules).string = {min_len: 1, max_len: 2048}];
}
```
C# error helper:
```
var br = new Google.Rpc.BadRequest();
br.FieldViolations.Add(new() { Field = "setting_id", Description = "must not be empty" });
var st = new Google.Rpc.Status { Code = (int)StatusCode.InvalidArgument, Message = "Validation failed" };
st.Details.Add(Google.Protobuf.WellKnownTypes.Any.Pack(br));
var md = new Grpc.Core.Metadata { { "grpc-status-details-bin", st.ToByteArray() } };
throw new Grpc.Core.RpcException(new Grpc.Core.Status(StatusCode.InvalidArgument, st.Message), md);
```

Optional runtime validation (pseudocode with protovalidate‑net):
```
// IValidator validator is DI‑registered from descriptors containing protovalidate rules
var result = validator.Validate(request);
if (!result.IsValid)
{
  var br = new Google.Rpc.BadRequest();
  foreach (var v in result.Violations)
    br.FieldViolations.Add(new() { Field = v.FieldPath, Description = v.Message });

  var status = new Google.Rpc.Status { Code = (int)StatusCode.InvalidArgument, Message = "Validation failed" };
  status.Details.Add(Any.Pack(br));
  var md = new Metadata { { "grpc-status-details-bin", status.ToByteArray() } };
  throw new RpcException(new Status(StatusCode.InvalidArgument, status.Message), md);
}
```

## Acceptance Criteria
- Every RPC has documented validation and at least one negative test.
- Services return canonical codes with `google.rpc` details; controllers translate to consistent HTTP responses.
- No SQL schemas or repository/data interfaces were changed.

## Repo Notes
- gRPC registrations: see `Authorization/Payment/*/DIExtensions.cs`, `Content/CMS/Services/DIExtensions.cs`, `Authorization/Events/Extensions/DIExtensions.cs`.
- Controllers exist (e.g., `Authentication/Services/Controllers/UserApiController.cs`) for HTTP translation work.
- Proto sources path (`Fragments/Protos/**/*.proto`) is not present in this repo. Existing error enums were inventoried from generated TS outputs in `Fragments/js/ts-gen/gen/Protos/...`.

---

## Existing Response Error Enums (Inventory)

Source: generated proto artifacts in `Fragments/js/ts-gen/gen/Protos/IT/WebServices/Fragments/**`. No `.proto` sources are present under `Fragments/Protos` in this repository at the time of this inventory.

- Settings
  - `ModifyResponseErrorType`
    - Fragments/js/ts-gen/gen/Protos/IT/WebServices/Fragments/Settings/SettingsInterface_pb.ts:14

- Authentication
  - `ChangeOtherPasswordResponse_ChangeOtherPasswordResponseErrorType`
    - Fragments/js/ts-gen/gen/Protos/IT/WebServices/Fragments/Authentication/UserInterface_pb.ts:187
  - `ChangeOtherProfileImageResponse_ChangeOtherProfileImageResponseErrorType`
    - Fragments/js/ts-gen/gen/Protos/IT/WebServices/Fragments/Authentication/UserInterface_pb.ts:301
  - `ChangeOwnPasswordResponse_ChangeOwnPasswordResponseErrorType`
    - Fragments/js/ts-gen/gen/Protos/IT/WebServices/Fragments/Authentication/UserInterface_pb.ts:413
  - `ChangeOwnProfileImageResponse_ChangeOwnProfileImageResponseErrorType`
    - Fragments/js/ts-gen/gen/Protos/IT/WebServices/Fragments/Authentication/UserInterface_pb.ts:519
  - `CreateUserResponse_CreateUserResponseErrorType`
    - Fragments/js/ts-gen/gen/Protos/IT/WebServices/Fragments/Authentication/UserInterface_pb.ts:659
  - `DisableEnableOtherUserResponse_DisableEnableOtherUserResponseErrorType`
    - Fragments/js/ts-gen/gen/Protos/IT/WebServices/Fragments/Authentication/UserInterface_pb.ts:767

Notes
- No additional `*ResponseErrorType` enums were found in Authorization, Page, Notification, or Events generated artifacts. Events contains an `EventError` message type but not a `*ResponseErrorType` enum.
- The C# `SettingsService` uses `ModifyResponseErrorType` in multiple return paths (e.g., Settings/Services/SettingsService.cs:166).

Next
- Use these enums as the canonical, non‑breaking error surface for responses. In C# services, set the enum value as today, and in addition attach gRPC `Status` + `google.rpc` details via `RpcException` to enrich clients that understand them.
- When proto sources become available under `Fragments/Protos`, repeat this scan to catch any stragglers and update this inventory.

---

## Domain Work Breakdown

Below is a concrete, domain‑by‑domain execution plan that preserves existing proto error enums while layering gRPC canonical status codes and google.rpc details in services, and consistent HTTP translation in controllers.

### Authentication
- Key services/files
  - gRPC services: `Authentication/Services/UserService.cs`, `Authentication/Services/ServiceService.cs`
  - Controller: `Authentication/Services/Controllers/UserApiController.cs`
- Existing proto error enums
  - `ChangeOtherPasswordResponse_ChangeOtherPasswordResponseErrorType`
  - `ChangeOtherProfileImageResponse_ChangeOtherProfileImageResponseErrorType`
  - `ChangeOwnPasswordResponse_ChangeOwnPasswordResponseErrorType`
  - `ChangeOwnProfileImageResponse_ChangeOwnProfileImageResponseErrorType`
  - `CreateUserResponse_CreateUserResponseErrorType`
  - `DisableEnableOtherUserResponse_DisableEnableOtherUserResponseErrorType`
- Mapping approach
  - Validation issues → `INVALID_ARGUMENT` with `BadRequest.field_violations`
  - Auth/authz issues → `UNAUTHENTICATED`/`PERMISSION_DENIED` with `ErrorInfo`
  - Not found (user, profile) → `NOT_FOUND` with `ResourceInfo`
  - Conflicts (username/email exists) → `ALREADY_EXISTS` with `ResourceInfo`
  - Business/state rules (password reuse, disabled) → `FAILED_PRECONDITION` with `PreconditionFailure`
  - Unknown → `INTERNAL`
- Service tasks
  - Add request validation helpers for password strength, image size/type, required IDs
  - Wrap repository null/existing checks into status mapping as above
  - Continue setting response enum values as defined by protos
- Controller tasks
  - Translate `RpcException` to HTTP 400/401/403/404/409/412/500 accordingly

### Settings
- Key services/files
  - gRPC services: `Settings/Services/SettingsService.cs`
- Existing proto error enums
  - `ModifyResponseErrorType` (multiple RPCs return messages with `Error` field of this enum)
- Mapping approach
  - Invalid keys/values → `INVALID_ARGUMENT` with `BadRequest`
  - Missing records → `NOT_FOUND` with `ResourceInfo`
  - Conflicts/immutable fields → `FAILED_PRECONDITION` or `ALREADY_EXISTS` (as appropriate)
  - Unknown → `INTERNAL`
- Service tasks
  - Centralize enum ↔ status mapping so each Modify* RPC sets enum and throws `RpcException` with details
  - Add guards for max value sizes, allowed characters, and allowed setting scopes

### Authorization / Payment
- Key services/files
  - gRPC services: `Authorization/Payment/*/*Service.cs`, `Authorization/Payment/Combined/Services/*.cs`
  - Registrations: `Authorization/Payment/*/DIExtensions.cs`, `Authorization/Payment/Combined/DIExtensions.cs`
- Existing proto error enums
  - Not detected in generated artifacts; rely on status codes + details
- Mapping approach
  - Validation (amounts, plan IDs) → `INVALID_ARGUMENT`
  - Business rules (expired, insufficient state) → `FAILED_PRECONDITION`
  - Provider errors (network) → `UNAVAILABLE` with optional `RetryInfo`
  - Duplicates → `ALREADY_EXISTS`
  - Missing records → `NOT_FOUND`
  - Auth issues → `UNAUTHENTICATED`/`PERMISSION_DENIED`
- Service tasks
  - Normalize downstream exceptions (Stripe, Paypal, Fortis) into canonical codes and attach `ErrorInfo.reason` with provider code
  - Ensure idempotency/concurrency maps to `ABORTED` where applicable

### Content / CMS
- Key services/files
  - gRPC services: `Content/CMS/Services/*.cs` (AssetService, ContentService, PageService, backups)
  - Registrations: `Content/CMS/Services/DIExtensions.cs`
- Existing proto error enums
  - Not detected; rely on status codes + details
- Mapping approach
  - Validation (slugs, titles, sizes, mime) → `INVALID_ARGUMENT`
  - Missing content/assets → `NOT_FOUND`
  - Conflicts (slug exists, version) → `ALREADY_EXISTS` or `ABORTED` for optimistic lock
  - Business rules (publish state) → `FAILED_PRECONDITION`
- Service tasks
  - Add validators for content fields and media constraints; convert repo results to status codes

### Content / Stats
- Key services/files
  - gRPC services: `Content/Stats/Services/*.cs` (Views, Likes, Shares, Progress, Save, Query)
- Existing proto error enums
  - Not detected; rely on status codes + details
- Mapping approach
  - Validation (page size, ids, date ranges) → `INVALID_ARGUMENT`
  - Missing content → `NOT_FOUND`
  - Business rules (subscription gating) → `FAILED_PRECONDITION` or `PERMISSION_DENIED`

### Notification
- Key services/files
  - gRPC services: `Notification/Services/*.cs`
- Existing proto error enums
  - Not detected; rely on status codes + details
- Mapping approach
  - Validation (channel/user ids) → `INVALID_ARGUMENT`
  - Missing subscriptions/users → `NOT_FOUND`
  - Provider/backends unavailability → `UNAVAILABLE` with `RetryInfo`

### Events
- Key services/files
  - gRPC services: `Authorization/Events/Services/*.cs`
  - Registrations: `Authorization/Events/Extensions/DIExtensions.cs`
- Existing proto error enums
  - None detected; `EventError` message exists but not a `*ResponseErrorType`
- Mapping approach
  - Validation (event ids, date ranges) → `INVALID_ARGUMENT`
  - Missing event/claims → `NOT_FOUND`
  - Business rules → `FAILED_PRECONDITION`

### Combined Services Host
- Key services/files
  - `Services/Combined/*.cs` (Program/Startup), registers multiple gRPC services
- Tasks
  - Ensure global interceptors/middleware can log and surface correlation IDs
  - Optionally add a common exception interceptor to unify `RpcException` construction if desired

### Per‑Domain Checklist Template
- Inventory RPCs and request messages
- Add request validators (or wire in PGV if available)
- Map repository results to canonical status codes
- Continue setting existing response enum fields; also attach google.rpc details
- Add negative tests for invalid args, not found, preconditions
- Update controller translations (if HTTP entrypoints exist)

---

## Domain TODO Checklists

Use these GitHub-style task lists to track execution. Keep this file as the single source of truth.

### Authentication
- RPC inventory
  - UserService
    - AuthenticateUser
    - ChangeOtherPassword
    - ChangeOtherProfileImage
    - ChangeOwnPassword
    - ChangeOwnProfileImage
    - CreateUser
    - DisableOtherUser
    - DisableOtherTotp
    - DisableOwnTotp
    - EnableOtherUser
    - GenerateOtherTotp
    - GenerateOwnTotp
    - GetAllUsers
    - GetOtherUser
    - GetOtherPublicUserByUserName
    - GetOtherTotpList
    - GetOwnTotpList
    - GetOwnUser
    - ModifyOtherUser
    - ModifyOtherUserRoles
    - ModifyOwnUser
    - RenewToken
    - SearchUsersAdmin
    - VerifyOtherTotp
    - VerifyOwnTotp
  - ServiceService
    - AuthenticateService
  - BackupService
    - RestoreAllData
- [ ] Add request validators (password rules, image constraints, required IDs)
- [ ] Add request validators (password rules, image constraints, required IDs)
- [ ] Map repo outcomes to status codes (`NOT_FOUND`, `ALREADY_EXISTS`, etc.)
- [ ] Preserve and set response enums (`Change*Response*ErrorType`, `CreateUser*ErrorType`)
- [ ] Attach `google.rpc` details via `RpcException` (BadRequest, ResourceInfo, PreconditionFailure, ErrorInfo)
- [ ] Add negative tests for invalid args, auth/authz failures, not found, preconditions
- [ ] Translate gRPC errors in `Authentication/Services/Controllers/UserApiController.cs`
- [ ] Add structured logging + correlation IDs at service boundaries

### Settings
- RPC inventory (SettingsService)
  - GetAdminData
  - GetAdminNewerData
  - GetOwnerData
  - GetOwnerNewerData
  - GetPublicData
  - GetPublicNewerData
  - ModifyCMSOwnerData
  - ModifyCMSPrivateData
  - ModifyCMSPublicData
  - ModifyCommentsOwnerData
  - ModifyCommentsPrivateData
  - ModifyCommentsPublicData
  - ModifyNotificationOwnerData
  - ModifyNotificationPrivateData
  - ModifyNotificationPublicData
  - ModifyPersonalizationOwnerData
  - ModifyPersonalizationPrivateData
  - ModifyPersonalizationPublicData
  - ModifySubscriptionOwnerData
  - ModifySubscriptionPrivateData
  - ModifySubscriptionPublicData
  - ModifyEventPublicSettings
- [ ] Add validators for key/value shapes and sizes
- [ ] Add validators for key/value shapes and sizes
- [ ] Map repo outcomes to status codes (`NOT_FOUND`, `ALREADY_EXISTS`, `FAILED_PRECONDITION`)
- [ ] Preserve and set `ModifyResponseErrorType` in responses
- [ ] Attach `google.rpc` details via `RpcException`
- [ ] Add negative tests (invalid key/value, not found, conflicts)
- [ ] If HTTP entrypoints exist, translate to consistent `ProblemDetails`
- [ ] Add structured logging + correlation IDs

### Authorization / Payment
- RPC inventory
  - Combined/PaymentService
    - CancelOwnSubscription
    - GetNewDetails
    - GetNewOneTimeDetails
    - GetOwnOneTimeRecord
    - GetOwnOneTimeRecords
    - GetOwnSubscriptionRecord
    - GetOwnSubscriptionRecords
    - ReconcileOwnSubscription
  - Combined/AdminPaymentService
    - BulkActionCancel
    - BulkActionStart
    - BulkActionStatus
    - CancelOtherSubscription
    - GetOtherOneTimeRecord
    - GetOtherOneTimeRecords
    - GetOtherSubscriptionRecord
    - GetOtherSubscriptionRecords
    - ReconcileOtherSubscription
  - Combined/ClaimsService
    - GetClaims
  - Combined/ServiceOpsService
    - ServiceStatus
  - Manual/ManualPaymentService
    - ManualCancelOtherSubscription
    - ManualCancelOwnSubscription
    - ManualGetOtherSubscriptionRecords
    - ManualGetOtherSubscriptionRecord
    - ManualGetOwnSubscriptionRecords
    - ManualGetOwnSubscriptionRecord
    - ManualNewOtherSubscription
    - ManualNewOwnSubscription
  - Paypal/PaypalService
    - PaypalNewOwnSubscription
  - Stripe/StripeService
    - StripeCheckOtherSubscription
    - StripeCheckOwnSubscription
    - StripeEnsureOneTimeProduct
  - Fortis/FortisService
    - FortisNewOwnSubscription
- [ ] Add validators (amounts, plan IDs, currency, idempotency keys)
- [ ] Add validators (amounts, plan IDs, currency, idempotency keys)
- [ ] Normalize provider errors (Stripe/Paypal/Fortis) to canonical status codes; set `ErrorInfo.reason` with provider code
- [ ] Map concurrency/idempotency conflicts to `ABORTED`
- [ ] Attach `google.rpc` details via `RpcException`; keep response shapes unchanged
- [ ] Add negative tests for validation, provider failures, not found, conflicts
- [ ] If HTTP entrypoints exist, add translation to `ProblemDetails`
- [ ] Add structured logging + correlation IDs

### Content / CMS
- RPC inventory
  - AssetService
    - CreateAsset
    - GetAsset
    - GetAssetAdmin
    - GetAssetByOldContentID
    - SearchAsset
    - GetImageAssets
  - PageService
    - CreatePage
    - DeletePage
    - GetAllPages
    - GetAllPagesAdmin
    - GetPage
    - GetPageByUrl
    - GetPageAdmin
    - ModifyPage
    - PublishPage
    - SearchPage
    - UndeletePage
    - UnpublishPage
  - ContentService
    - AnnounceContent
    - CreateContent
    - DeleteContent
    - GetAllContent
    - GetAllContentAdmin
    - GetContent
    - GetContentByUrl
    - GetContentAdmin
    - GetRecentCategories
    - GetRecentTags
    - GetRelatedContent
    - ModifyContent
    - PublishContent
    - SearchContent
    - UnannounceContent
    - UndeleteContent
    - UnpublishContent
  - ContentBackupService
    - RestoreAllData
- [ ] Add validators (slugs, titles, mime/types, size limits)
- [ ] Add validators (slugs, titles, mime/types, size limits)
- [ ] Map repo outcomes to status codes (`NOT_FOUND`, `ALREADY_EXISTS`, `ABORTED`, `FAILED_PRECONDITION`)
- [ ] Attach `google.rpc` details via `RpcException`
- [ ] Add negative tests (invalid args, not found, conflicts, preconditions)
- [ ] If HTTP entrypoints exist, translate to `ProblemDetails`
- [ ] Add structured logging + correlation IDs

### Content / Stats
- RPC inventory
  - LikeService: LikeContent, UnlikeContent
  - ProgressService: LogProgressContent
  - QueryFileService: GetContentStats, GetOtherUserStats, GetOwnUserLikes, GetOwnUserProgressHistory, GetOwnUserSaves, GetOwnUserStats
  - QuerySqlService: GetContentStats, GetOwnUserSaves
  - SaveService: SaveContent, UnsaveContent
  - ServiceOpsService: ServiceStatus
  - ShareService: LogShareContent
  - ViewService: LogViewContent
- [ ] Add validators (ids, pagination, date ranges)
- [ ] Add validators (ids, pagination, date ranges)
- [ ] Map gating/business rules to `PERMISSION_DENIED`/`FAILED_PRECONDITION`
- [ ] Attach `google.rpc` details via `RpcException`
- [ ] Add negative tests (invalid args, not found, authz)
- [ ] If HTTP entrypoints exist, translate to `ProblemDetails`
- [ ] Add structured logging + correlation IDs

### Notification
- RPC inventory
  - NotificationService: SendEmail
  - UserService: GetAllTokens, GetRecord, ModifyNormalRecord, RegisterNewToken, UnRegisterNewToken
- [ ] Add validators (channel IDs, user IDs, payload limits)
- [ ] Add validators (channel IDs, user IDs, payload limits)
- [ ] Map provider/backends downtime to `UNAVAILABLE` (+ `RetryInfo`)
- [ ] Attach `google.rpc` details via `RpcException`
- [ ] Add negative tests (invalid args, not found, provider failures)
- [ ] If HTTP entrypoints exist, translate to `ProblemDetails`
- [ ] Add structured logging + correlation IDs

### Events
- RPC inventory
  - AdminEventService
    - AdminCreateEvent
    - AdminCreateRecurringEvent
    - AdminGetEvent
    - AdminGetEvents
    - AdminModifyEvent
    - AdminCancelEvent
    - AdminCancelAllRecurringEvents
    - AdminGetTicket
    - AdminGetTicketsForEvent
    - AdminCancelOtherTicket
    - AdminReserveEventTicketForUser
  - ClaimsService: GetClaims
  - EventService: GetEvent, GetEvents, GetOwnTicket, GetOwnTickets, CancelOwnTicket, ReserveTicketForEvent, UseTicket
- [ ] Add validators (event IDs, ranges)
- [ ] Add validators (event IDs, ranges)
- [ ] Map repo outcomes and state rules to canonical status codes
- [ ] Attach `google.rpc` details via `RpcException`
- [ ] Add negative tests (invalid args, not found, preconditions)
- [ ] If HTTP entrypoints exist, translate to `ProblemDetails`
- [ ] Add structured logging + correlation IDs

### Combined Host
- [ ] Ensure global interceptors/middleware for `RpcException` consistency and correlation IDs
- [ ] Verify DI registration for all updated services
- [ ] Add high-level smoke tests for error translation behavior across services

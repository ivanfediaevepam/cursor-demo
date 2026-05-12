## Context

**AviationApi** exposes `PUT /flights/{id}/status` with `UpdateFlightStatusRequest` (status only). The `Flight` entity and in-memory `FlightRepository` persist status but have no delay metadata. The **React** app types flights in `src/frontend/src/services/Flight.ts` and shows status in components such as `FlightDetails` and `FlightStatusBadge`, without any delay-reason capture UI today.

## Goals / Non-Goals

**Goals:**

- Persist a **delay reason** when (and only when) the flight’s status is **Delayed**, and return it on flight reads and on successful status updates.
- Validate delay reason on the server for **Delayed** transitions (reject invalid or missing reason per spec).
- Show delay reason in the UI for delayed flights and collect it when the user selects **Delayed** (same validation on the client for fast feedback).

**Non-Goals:**

- Historical audit log of reason changes, notifications, or integration with external ops systems.
- Database migrations (repository remains in-memory); no new NuGet packages.

## Decisions

1. **Field shape** — Add nullable `DelayReason` (`delayReason` in JSON) on `Flight` and on `UpdateFlightStatusRequest` (or a dedicated property only used when `Status == Delayed`). Rationale: one read model for clients; simple serialization with System.Text.Json defaults already used by the API.

2. **Reason values** — Use a fixed **vocabulary** of string reasons aligned with the examples (`Weather`, `Technical`, and at least one more such as `Operational` or `AirTraffic`) plus optional `Other` if free text is required later. Implement as server-side validation (allowed set) to avoid arbitrary junk in the demo dataset; mirror as a TypeScript union or const array on the frontend. Rationale: matches user examples without introducing a new enum serialization policy project-wide.

3. **When status leaves Delayed** — On any successful status update where the new status is not **Delayed**, **clear** `DelayReason` to `null` so the API does not imply a current delay. Rationale: keeps responses truthful and simplifies the UI.

4. **Required vs optional on non-Delayed** — For non-**Delayed** statuses, **ignore** `DelayReason` in the body (or treat as null). For **Delayed**, **require** a non-empty reason that passes vocabulary validation. Rationale: satisfies “users should be able to provide a reason” when delaying.

5. **Repository** — Extend `FlightRepository.UpdateFlight` to copy `DelayReason` like other scalar fields. Seed flights in the repository constructor: leave `DelayReason` null unless a sample delayed flight is useful for demos.

6. **Frontend** — Extend the `Flight` interface; when implementing status change UI, use a select (or radio group) bound to the same vocabulary; if no central API helper exists yet, add a small function in the service layer that calls `PUT /flights/{id}/status` with the extended body.

## Risks / Trade-offs

- **[Risk]** Clients that do not send `delayReason` when delaying will receive **400** after this change → **Mitigation**: update the React client and document the field in OpenAPI if the project documents this endpoint.
- **[Risk]** Fixed vocabulary may be too narrow for real ops → **Mitigation**: spec marks vocabulary as product-controlled; `Other` can be added in a follow-up without breaking nullable semantics.
- **[Trade-off]** In-memory storage only: reason is lost on process restart → acceptable for this demo codebase.

## Migration Plan

- Deploy API and frontend together (or API first only if clients tolerate new optional field on responses; **Delayed** updates require the new body field once validation is on).
- No data backfill required; existing flights keep `delayReason: null`.
- **Rollback**: revert API validation and field; clients stop sending `delayReason`.

## Open Questions

- Exact label list for the vocabulary (product copy): default to `Weather`, `Technical`, `Operational` unless stakeholders prefer different terms.
- Whether the UI for changing status already exists on a specific page; implementation tasks should locate the actual status-update entry point or add one if missing.

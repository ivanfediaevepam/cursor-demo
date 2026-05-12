## Why

Operations teams need an auditable explanation when a flight is marked **Delayed** so dispatch, crew, and passengers see *why* the delay occurred (for example weather or a technical issue), not only that the flight is delayed.

## What Changes

- Extend the flight status update flow so that when status becomes **Delayed**, the client sends a **delay reason** from a controlled vocabulary (for example `Weather`, `Technical`). On the **.NET `Flight` model**, `DelayReason` remains **optional / nullable** (`string?`): it is `null` whenever the flight is not delayed (including all non-delayed statuses and legacy rows); it is populated only while status is **Delayed**.
- Persist that nullable **delay reason** on the flight resource in **AviationApi** (model, repository update path, and JSON contract as `delayReason: string | null`).
- Return **delay reason** on flight GET responses and surface it in the **React** UI wherever delayed flights are shown (detail views, badges, or status panels as appropriate).
- Add validation on the **status update** endpoint: when the requested status is **Delayed**, a valid reason is **required** in the request body; for other statuses the property is omitted or ignored and stored delay reason is cleared. Allowed values are enforced when a reason is supplied for a delayed transition.
- Add or extend **unit tests** on the controller/repository and frontend tests where status is displayed.

## Capabilities

### New Capabilities

- `flight-delay-reason`: Capture, validate, persist, and expose a delay reason whenever flight status is **Delayed**; align API and UI contracts.

### Modified Capabilities

- None (no existing capability specs under `openspec/specs/`).

## Impact

- **Backend**: `Flight`, `UpdateFlightStatusRequest`, `FlightsController` status update action, `FlightRepository.UpdateFlight`, seed/sample data if any, OpenAPI/Swagger if present, `AvationApi.Tests` controller tests.
- **Frontend**: `Flight` TypeScript type, components that render flight status (for example `FlightDetails`, `FlightStatusBadge` or parent pages), any form or control that sets status to **Delayed**, `FlightService` or equivalent API client.
- **API contract**: Nullable `delayReason` on the flight resource and on the status PUT body; `null`/missing is normal for non-delayed flights; clients must tolerate `null`/missing for older flights until backfilled.

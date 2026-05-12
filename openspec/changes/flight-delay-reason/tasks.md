## 1. Backend model and persistence

- [x] 1.1 Add **`string?` `DelayReason`** to `Flight` (always nullable on the entity—only meaningful when `Status == Delayed`; otherwise `null`). On `UpdateFlightStatusRequest`, add nullable **`string?` `DelayReason`** as well: optional on the type, but **validation requires** a valid non-null reason when `Status == Delayed`; omit or null for other statuses. Serialize as camelCase `delayReason` in JSON.
- [x] 1.2 Introduce a small shared validator or private helper on `FlightsController` (or a dedicated validator type) that defines the allowed delay-reason vocabulary and validates non-empty reason when `Status == Delayed`.
- [x] 1.3 Update `FlightsController.UpdateFlightStatus`: when transition is to **Delayed**, require valid reason; when transition is to any other status, set `DelayReason` to null before save; persist reason on the entity.
- [x] 1.4 Update `FlightRepository.UpdateFlight` to copy `DelayReason`; adjust in-memory seed flights if any should demo a delayed flight with reason.

## 2. Backend tests and API docs

- [x] 2.1 Extend `FlightsControllerTests` (or equivalent) for: delayed with valid reason succeeds; delayed without reason returns 400; delayed with invalid reason returns 400; transition from Delayed to another status clears reason in returned flight.
- [x] 2.2 If the project uses Swagger/OpenAPI annotations on this action, document `DelayReason` and error responses (optional follow-up per repo convention).

## 3. Frontend types and API

- [x] 3.1 Add optional `delayReason` to the `Flight` interface in `Flight.ts` and export the same vocabulary type/const list as the backend for the select control.
- [x] 3.2 Implement or extend the HTTP call to `PUT /flights/{id}/status` so the body includes `delayReason` when status is **Delayed**; locate or add the UI entry point where operators change flight status.

## 4. Frontend UI and tests

- [x] 4.1 When the user chooses **Delayed**, show a required control (select) for delay reason; block submit until chosen; send value in the status update request.
- [x] 4.2 Display `delayReason` next to delayed status in `FlightDetails`, `FlightStatusBadge`, or the parent that owns layout—without breaking non-delayed views.
- [x] 4.3 Add or update Vitest/RTL coverage for delayed + reason display and for validation/disable behavior when delaying.

## ADDED Requirements

### Requirement: Delay reason is captured when a flight is marked delayed

The system SHALL accept a delay reason when a flight’s status is set to **Delayed** and SHALL persist it on the flight resource.

#### Scenario: Successful delay with valid reason

- **WHEN** a client sets a flight’s status to **Delayed** with a delay reason that belongs to the configured allowed vocabulary
- **THEN** the system stores that reason on the flight and returns it on subsequent reads of that flight

#### Scenario: Delay rejected without reason

- **WHEN** a client sets a flight’s status to **Delayed** without a delay reason or with a blank reason
- **THEN** the system SHALL NOT apply the status change and SHALL respond with an error indicating that a delay reason is required

#### Scenario: Delay rejected with invalid reason

- **WHEN** a client sets a flight’s status to **Delayed** with a delay reason that is not in the allowed vocabulary
- **THEN** the system SHALL NOT apply the status change and SHALL respond with an error indicating the reason is invalid

### Requirement: Delay reason is cleared when the flight is no longer delayed

The system SHALL NOT retain a stale delay reason on the flight once the status is no longer **Delayed**.

#### Scenario: Transition away from delayed clears reason

- **WHEN** a client successfully updates a flight from **Delayed** to any other allowed status
- **THEN** the stored delay reason for that flight SHALL be cleared (empty / null in the API contract)

### Requirement: Delay reason is visible to API consumers and the web UI

The system SHALL expose the current delay reason whenever a flight is returned by the API and the web application SHALL display it for delayed flights.

#### Scenario: API includes delay reason for delayed flights

- **WHEN** a flight’s status is **Delayed** and a delay reason is stored
- **THEN** any API response that includes that flight’s representation SHALL include the delay reason field with the stored value

#### Scenario: UI shows reason for delayed flights

- **WHEN** a user views a flight whose status is **Delayed** and a delay reason is present
- **THEN** the user interface SHALL show that delay reason alongside or adjacent to the delayed status in human-readable form

# PUT `/flights/{id}/status` — sequence flow

This diagram shows how a client update moves through `FlightsController`, the request and domain models, and `FlightRepository` for the status update endpoint (`FlightsController.UpdateFlightStatus`).

**Scope:** Request enters the API, the body is bound to `UpdateFlightStatusRequest`, the controller loads a `Flight`, validates the requested `FlightStatus` transition, mutates the entity, persists via the repository, and returns the updated `Flight`. Error responses (400/404) are summarized in the `alt` branch.

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant FC as FlightsController
    participant Req as UpdateFlightStatusRequest
    participant FR as FlightRepository
    participant Fl as Flight

    Client->>FC: PUT /flights/{id}/status JSON body
    Note over FC, Req: ASP.NET Core binds body to UpdateFlightStatusRequest

    FC->>FC: validate request != null
    opt request body is null
        FC-->>Client: 400 Bad Request
    end

    FC->>FR: GetFlightById(id)
    FR-->>FC: Flight or null
    opt no flight for id
        FC-->>Client: 404 Not Found
    end

    FC->>Req: read Status (target state)
    FC->>Fl: read Status, DepartureTime (current state)
    FC->>FC: apply transition rules (switch on new status)

    alt invalid transition, bad timing, or unsupported status
        FC-->>Client: 400 Bad Request
    else transition allowed
        FC->>Fl: assign Status from request
        FC->>FR: UpdateFlight(flight)
        FR->>Fl: copy fields from flight onto stored instance
        FR-->>FC: Flight
        FC-->>Client: 200 OK with Flight body
    end
```

## Model roles

| Type | Role |
|------|------|
| `UpdateFlightStatusRequest` | Input DTO; supplies the desired `FlightStatus` after JSON deserialization. |
| `Flight` | Aggregate loaded from the repository; controller updates `Status` after rules pass; repository writes all fields back to the in-memory store. |

# Aviation Demo — system architecture

High-level **container view** (C4-style): what runs where, and how the main pieces talk to each other. Intended for onboarding (“what talks to what?”).

```mermaid
flowchart TB
    User(("Browser user"))

    subgraph System["Aviation Demo system boundary"]
        direction TB

        subgraph FE["Container: Web UI — React + Vite"]
            SPA["React SPA<br/>(pages, components, client HTTP)"]
        end

        subgraph BE["Container: HTTP API — ASP.NET Core"]
            direction TB
            Host["Kestrel host<br/>middleware + routing + OpenAPI"]
            subgraph API["API layer"]
                Controllers["Controllers<br/>(e.g. Flights, Planes)"]
            end
            subgraph Data["In-memory persistence (same process)"]
                Repo["IFlightRepository → FlightRepository"]
                Store[("In-memory flight list<br/>(no external database)")]
            end
        end
    end

    User -->|opens| SPA
    SPA -->|HTTPS / JSON REST| Host
    Host --> Controllers
    Controllers -->|domain calls| Repo
    Repo -->|read / write| Store

    classDef person fill:#e8f5e9,stroke:#2e7d32
    classDef container fill:#e3f2fd,stroke:#1565c0
    classDef process fill:#fff8e1,stroke:#f57f17
    classDef store fill:#fce4ec,stroke:#c2185b

    class User person
    class SPA,Host container
    class Controllers,Repo process
    class Store store
```

## Notes

- **Frontend** (`src/frontend`): Vite dev server or static production build; talks to the API over HTTP with JSON (e.g. flight resources).
- **Backend** (`src/backend/AviationApi`): Single ASP.NET Core process; controllers depend on **repository interfaces** implemented by **in-memory** types (`FlightRepository` holds a `List<Flight>` seeded in code).
- **No separate database container** — persistence lives inside the API process only; restarting the API resets in-memory data unless the implementation changes.

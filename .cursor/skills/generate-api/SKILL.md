---
name: Generate .NET API Endpoint
description: Scaffold new ASP.NET Core API endpoints (controllers and action methods) in the AviationApi backend that match existing project conventions — controller location, constructor injection, `ActionResult<T>` returns, standard HTTP responses, input validation, and no new NuGet dependencies. Use whenever the user asks to add, create, scaffold, generate, or implement an endpoint, controller, action, route, or REST handler in the .NET backend. Triggers include any verb+path pair ("POST /flights", "GET /aircraft/{id}"), or phrasing like "add a … endpoint", "expose … via the API", "new handler for …". Accepts a free-form instruction describing verb, route, behavior, and contract, and respects it. Does NOT add OpenAPI/Swagger annotations — that is handled by the Create Swagger Doc skill, which is run separately afterward.
---

# Generate .NET API Endpoint

Generate code that **looks like it was already in the project**. The success criterion is that an existing maintainer would not be able to tell which endpoint Claude wrote and which were there before — same conventions, same shapes, same idioms.

## Invocation pattern

Typically invoked with a free-form instruction, e.g.

> `/generate-api Add a PUT /flights/{id}/status endpoint to update a flight's status. It should check if the flight exists (returning 404 if not), update the status, and return 200 OK with the updated flight.`

Parse the instruction into five fields before writing code:

1. **HTTP verb** (`PUT`)
2. **Route template** (`/flights/{id}/status`)
3. **Request shape** (a status value — body or path? confirm against existing conventions)
4. **Response shape** (the updated flight)
5. **Status-code branches** (200 success, 404 not found, plus any implicit ones from validation)

If any of these is genuinely ambiguous and the project's existing patterns don't resolve it, ask one targeted question. Otherwise pick the option that best matches surrounding code and state the assumption inline.

## Workflow

1. **Inspect before writing.** Open `src/backend/AviationApi/Controllers/` and read at least one existing controller for the same or a similar resource. Note: routing style, base class, attribute set, constructor injection pattern, async style, validation idiom, error-mapping idiom, DTO vs entity in responses, cancellation-token usage, authorization attributes.
2. **Decide controller placement.** Same resource as an existing controller → extend it. New resource → new file `{Resource}sController.cs` (plural noun, PascalCase) following the conventions seen in step 1.
3. **Identify the collaborator repository.** Look for the existing repository interface for the resource (e.g. `IFlightRepository`). Find the methods you need. If a needed method does not exist, **stop and flag the gap** rather than inventing a repository signature — adding repository methods is out of scope for this skill unless the user explicitly asks.
4. **Write the action method** following the verb pattern below.
5. **Add input validation** — null/empty checks, range checks, ID format. Use the project's validation idiom (annotation-based via `[ApiController]` auto-400, or manual `BadRequest("...")`).
6. **Verify** against the checklist before returning.

## Project conventions

- **Location:** `src/backend/AviationApi/Controllers/`
- **File name:** `{Resource}sController.cs` (plural).
- **Class declaration:** `[ApiController]` + `[Route("api/[controller]")]` (or the project's actual route prefix — verify in step 1).
- **Dependencies:** constructor injection only. No service locator. No `new` of services/repositories inside actions.
- **Return type:** `ActionResult<T>` for actions returning a body; `ActionResult` for ones that don't. (Note: the existing controllers are synchronous, do not use `Task` unless the repository method is async).
- **Status helpers:** `Ok(...)`, `CreatedAtAction(...)`, `NoContent()`, `BadRequest(...)`, `NotFound()`, `Conflict(...)`. Don't hand-roll `StatusCode(...)` calls if a helper exists.
- **Entities over DTOs** in requests/responses unless the existing controllers use DTOs (e.g. `FlightsController` uses `Flight` directly).
- **No new NuGet packages.** If a problem appears to need one, solve it with what's already referenced or flag it.

## Verb-specific patterns

### GET single

```csharp
[HttpGet("{id:int}")]
public ActionResult<Flight> GetById(int id)
{
    if (id <= 0) return BadRequest("Id must be positive.");

    var flight = _flightRepository.GetFlightById(id);
    return flight is null ? NotFound() : Ok(flight);
}
```

### GET list

```csharp
[HttpGet]
public ActionResult<List<Flight>> GetAll()
{
    var flights = _flightRepository.GetAllFlights();
    return Ok(flights);
}
```

### POST create

```csharp
[HttpPost]
public ActionResult<Flight> Create([FromBody] Flight request)
{
    if (request is null) return BadRequest("Body is required.");

    _flightRepository.AddFlight(request);
    return CreatedAtAction(nameof(GetById), new { id = request.Id }, request);
}
```

### PUT replace / update

```csharp
[HttpPut("{id:int}")]
public ActionResult<Flight> Update(
    int id,
    [FromBody] Flight request)
{
    if (request is null) return BadRequest("Body is required.");

    var existing = _flightRepository.GetFlightById(id);
    if (existing is null) return NotFound();

    _flightRepository.UpdateFlight(request);
    return Ok(request);
}
```

### DELETE

```csharp
[HttpDelete("{id:int}")]
public ActionResult Delete(int id)
{
    var existing = _flightRepository.GetFlightById(id);
    if (existing is null) return NotFound();

    _flightRepository.DeleteFlight(id);
    return NoContent();
}
```

## Worked example — matching the invocation

> `Add a PUT /flights/{id}/status endpoint…`

**Assumption stated inline** (because the verb is `PUT` against a sub-resource): the new status is sent in the request body as a small DTO, not as a path/query param. This matches the convention of using bodies for state changes.

```csharp
public sealed record UpdateFlightStatusRequest(string Status);

// inside FlightsController
[HttpPut("{id:int}/status")]
public ActionResult<Flight> UpdateStatus(
    int id,
    [FromBody] UpdateFlightStatusRequest request)
{
    if (request is null || string.IsNullOrWhiteSpace(request.Status))
        return BadRequest("Status is required.");

    var flight = _flightRepository.GetFlightById(id);
    if (flight is null) return NotFound();

    flight.Status = Enum.Parse<FlightStatus>(request.Status);
    _flightRepository.UpdateFlight(flight);
    return Ok(flight);
}
```

If `_flightRepository.UpdateFlight` does not exist, flag it before generating — do not silently add the method to the repository. Place `UpdateFlightStatusRequest` wherever the project keeps request DTOs (often `Models/` or alongside other types — verify in step 1, do not invent a new folder).

## Routing rules

- Plural resource names (`/flights`, not `/flight`).
- Sub-resources via nested routes (`/flights/{id}/status`, `/aircraft/{id}/maintenance-logs`).
- For non-CRUD operations, use a verb segment after the id (`/flights/{id}/cancel`) rather than inventing a top-level action route.
- Apply route constraints where the type is known (`{id:int}`, `{code:length(3)}`).
- Match the casing convention seen in existing controllers (kebab-case vs camelCase in path segments).

## Validation checklist

Before finishing, verify:

- [ ] Controller is in `src/backend/AviationApi/Controllers/` with the correct file/class name.
- [ ] Class has the same attributes (`[ApiController]`, `[Route(...)]`, any `[Authorize]`) as siblings.
- [ ] Constructor injection used; no new fields beyond what's needed.
- [ ] Return type is `ActionResult<T>` (with body) or `ActionResult` (without), consistent with siblings.
- [ ] Every code path ends in a status-helper call (`Ok`, `NotFound`, `BadRequest`, `CreatedAtAction`, `NoContent`, `Conflict`).
- [ ] Input validation exists for IDs, required fields, and request bodies; messages are descriptive.
- [ ] All called repository methods exist; gaps are flagged, not silently filled.
- [ ] No new NuGet packages.
- [ ] No `Task` or `async` unless the underlying repository methods are actually asynchronous.
- [ ] Authorization attributes match the controller's existing policy (don't tighten or loosen by accident).
- [ ] Entities vs DTOs in requests/responses match siblings.
- [ ] No Swagger/OpenAPI annotations added (separate skill handles those).

## What to avoid

- Adding NuGet packages of any kind.
- Inventing repository methods rather than flagging the gap.
- Introducing new architectural patterns (mediator, minimal APIs, endpoint filters) the project doesn't already use.
- Mixing attribute-based and convention-based routing within a single controller.
- Wrapping every action in `try/catch` unless the project's pattern does so.
- Adding `[ProducesResponseType]` / `[SwaggerOperation]` here — the Create Swagger Doc skill handles those.
- Modifying unrelated existing actions.
- Creating new folders for requests/DTOs when an existing one is in use.

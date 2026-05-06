---
name: Test Generation .NET
description: Author xUnit unit tests in C# for the AviationApi backend using FluentAssertions for assertions and NSubstitute for mocking, mirroring the main project's folder structure with no new NuGet dependencies. Use whenever the user asks to write, generate, scaffold, or add tests for a .NET class, controller, service, repository, handler, or method; or mentions xUnit, `[Fact]`, `[Theory]`, FluentAssertions, NSubstitute, mocking, AAA, "happy path", or "error case" in a backend context. Accepts a free-form instruction describing the SUT, the methods in scope, and the scenarios to cover, and respects it.
---

# Test Generation .NET

Write tests that **describe behavior, not implementation**. A good test fails for one specific reason and reads top-to-bottom as a sentence about what the system does. Avoid brittle tests that re-state the SUT's code, and avoid sprawling tests that cover six concerns in one method.

> Note on the original path `src/backend/AvationApi.Tests/`: this looks like a typo (`Avation` vs `Aviation`). This skill uses `src/backend/AviationApi.Tests/` to match the main project name `AviationApi`. If the actual folder on disk is the typo'd version, use whatever exists and flag the inconsistency.

## Invocation pattern

Typically called with a free-form instruction, e.g.

> `/test-generation-dotnet Generate a full xUnit test class for FlightsController. Cover the new PUT status method specifically, testing both the successful update scenario and the 404 not found scenario.`

Parse it into:

1. **System under test (SUT)** — class and constructor dependencies.
2. **Methods in scope** — explicit list, or "all public methods", or "the new PUT method".
3. **Scenarios** — happy path, validation, not-found, conflict, exception, authorization, etc.
4. **Test data** — provided by the user, drawn from existing fixtures, or invented inline.

If the user names specific scenarios (e.g. "200 and 404 only"), generate exactly those — don't silently add a "BadRequest when body is null" test the user didn't ask for.

## Workflow

1. **Inspect the SUT.** Read the class. List its dependencies (constructor parameters), the methods in scope, every `return` branch, and every thrown exception. Each branch becomes at least one test.
2. **Inspect a sibling test class.** Read at least one existing `*Tests.cs` to match: file location, namespace, class naming, method naming, AAA convention (comments vs blank lines), mock-setup style, and any `IClassFixture` / collection fixtures already in use.
3. **Locate or invent test data.** Prefer existing builders / object mothers / fixtures over inline literals. Don't invent a builder if siblings use inline literals.
4. **Plan the test list** before writing any code. One test per behavior. Name them so the list reads as a behavior spec.
5. **Write the test class** following the structure below.
6. **Verify** against the validation checklist.

## Project conventions

- **Location:** `src/backend/AviationApi.Tests/{MirroredFolder}/{Class}Tests.cs`. Mirror the main project — `Controllers/FlightsController.cs` → `Controllers/FlightsControllerTests.cs`.
- **Namespace:** mirrors the SUT's namespace with `.Tests` appended (or whatever the project's existing convention is — verify).
- **Class naming:** `{ClassUnderTest}Tests`.
- **Method naming:** `Method_Scenario_ExpectedBehavior` (e.g. `UpdateStatus_WhenFlightExists_ReturnsOkWithUpdatedFlight`). Match siblings if they use a different convention (`Should_X_When_Y`, plain English, etc.).
- **One test class per SUT.** Don't combine multiple SUTs into one file.
- **No new NuGet packages.** xUnit + FluentAssertions + NSubstitute are the only test dependencies; don't add Moq, AutoFixture, Bogus, FsCheck, etc.

## Test class skeleton

xUnit instantiates the test class **once per test method**, so the constructor is the per-test setup point — there is no `[SetUp]`/`[TestInitialize]`.

```csharp
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace AviationApi.Tests.Controllers;

public class FlightsControllerTests
{
    private readonly IFlightRepository _flightRepository;
    private readonly FlightsController _sut;

    public FlightsControllerTests()
    {
        _flightRepository = Substitute.For<IFlightRepository>();
        _sut = new FlightsController(_flightRepository);
    }

    // [Fact] / [Theory] methods follow…
}
```

- **Mocks are private readonly fields**, instantiated in the constructor.
- **`_sut` is conventional** for "system under test" but follow what siblings use (`_controller`, `_service`, etc.).
- **Implement `IDisposable`** only if there's something to clean up (open streams, temp files). Plain unit tests don't need it.

## xUnit patterns

- **`[Fact]`** for a single, parameter-less behavior test.
- **`[Theory]` + `[InlineData(...)]`** for the same behavior across small data sets.
- **`[Theory]` + `[MemberData(nameof(Source))]`** for typed or computed data sets that don't fit `InlineData`'s primitive constraint.
- **`[Trait("Category", "Unit")]`** if siblings use traits for filtering.
- **`IClassFixture<T>`** when expensive setup (e.g., in-memory DB) should be shared across tests in _one_ class. Avoid for plain unit tests.
- **No test ordering.** xUnit runs tests in unspecified order; tests that depend on order are bugs.

```csharp
[Theory]
[InlineData("Scheduled")]
[InlineData("Boarding")]
[InlineData("Delayed")]
public void UpdateStatus_WithValidStatus_ReturnsOk(string status)
{
    // …
}
```

## NSubstitute patterns

```csharp
// Set up a return value
_flightRepository.GetFlightById(42).Returns(existingFlight);

// Set up an exception
_flightRepository.When(x => x.UpdateFlight(Arg.Any<Flight>()))
    .Do(x => throw new InvalidOperationException("conflict"));

// Verify a call
_flightRepository.Received(1).UpdateFlight(Arg.Any<Flight>());

// Verify a call did NOT happen — critical for negative paths
_flightRepository.DidNotReceive().DeleteFlight(Arg.Any<int>());

// Argument matchers
_flightRepository.GetFlightById(Arg.Is<int>(id => id > 0)).Returns(...);
```

- **Returns((T?)null)** is the right shape for nullable returns — cast explicitly so the compiler picks the right overload.
- **Mock interfaces, not concrete classes.** If the SUT depends on a concrete class, that's a code-smell to flag, not a reason to use a different mocking library.

## FluentAssertions patterns

```csharp
// Equality / shape
result.Should().Be(expected);
result.Should().BeEquivalentTo(expected);                  // structural
result.Should().BeNull();
result.Should().NotBeNull();

// Type checks
actionResult.Should().BeOfType<OkObjectResult>();
actionResult.Should().BeAssignableTo<IActionResult>();

// Chained drilling (BeOfType returns the typed value via .Which)
actionResult.Should().BeOfType<OkObjectResult>()
    .Which.Value.Should().BeEquivalentTo(updatedDto);

// Collections
items.Should().HaveCount(3);
items.Should().ContainSingle(f => f.Id == 42);
items.Should().BeInAscendingOrder(f => f.DepartureTime);

// Exceptions
act.Should().Throw<InvalidOperationException>()
    .WithMessage("*conflict*");
act.Should().NotThrow();
```

- **Prefer `BeEquivalentTo` over `Be`** for DTO comparisons — it compares structure, not reference.
- **Use the `because` parameter** when a failure message wouldn't be self-explanatory: `result.Should().BeNull(because: "the lookup should miss for unknown ids")`.
- **One conceptual assertion per test.** Multiple `.Should()` calls on the same concept (status code + body shape + interaction count) are fine; mixing two unrelated concepts isn't.

## Controller-specific patterns

Actions returning `ActionResult<T>` produce a wrapper. Drill in via `.Result`:

```csharp
// 200 OK with body
var result = _sut.GetById(1);
result.Result.Should().BeOfType<OkObjectResult>()
    .Which.Value.Should().BeEquivalentTo(expectedDto);

// 404 — no body, type check is enough
var result = _sut.GetById(999);
result.Result.Should().BeOfType<NotFoundResult>();

// 400 BadRequest with message
var result = _sut.Create(invalidRequest);
result.Result.Should().BeOfType<BadRequestObjectResult>()
    .Which.Value.Should().Be("Body is required.");

// 201 Created
var result = _sut.Create(validRequest);
result.Result.Should().BeOfType<CreatedAtActionResult>()
    .Which.Value.Should().BeEquivalentTo(createdDto);
```

For non-`ActionResult<T>` actions returning `IActionResult` directly, drop the `.Result` access.

## Worked example — matching the invocation

> `Cover the new PUT status method… successful update scenario and 404 not found scenario.`

```csharp
using AviationApi.Controllers;
using AviationApi.Models;
using AviationApi.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace AviationApi.Tests.Controllers;

public class FlightsControllerTests
{
    private readonly IFlightRepository _flightRepository;
    private readonly FlightsController _sut;

    public FlightsControllerTests()
    {
        _flightRepository = Substitute.For<IFlightRepository>();
        _sut = new FlightsController(_flightRepository);
    }

    [Fact]
    public void UpdateStatus_WhenFlightExists_ReturnsOkWithUpdatedFlight()
    {
        // Arrange
        const int flightId = 42;
        var existing = new Flight { Id = flightId, Status = FlightStatus.Scheduled };
        
        _flightRepository.GetFlightById(flightId).Returns(existing);

        // Act
        var result = _sut.UpdateFlightStatus(flightId, FlightStatus.Boarding);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo($"Flight status updated to {FlightStatus.Boarding}.");

        _flightRepository.Received(1).UpdateFlight(Arg.Is<Flight>(f => f.Status == FlightStatus.Boarding));
    }

    [Fact]
    public void UpdateStatus_WhenFlightDoesNotExist_ReturnsNotFoundAndDoesNotUpdate()
    {
        // Arrange
        const int flightId = 999;
        _flightRepository.GetFlightById(flightId).Returns((Flight?)null);

        // Act
        var result = _sut.UpdateFlightStatus(flightId, FlightStatus.Boarding);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _flightRepository.DidNotReceive()
            .UpdateFlight(Arg.Any<Flight>());
    }
}
```

Notes on what this does and doesn't do:

- **Two tests, one per scenario** — exactly what was asked for.
- **The 404 test verifies `DidNotReceive`** for `UpdateStatusAsync` — without it, a regression that calls update _before_ the existence check would still pass the type check on the result.
- **`BeEquivalentTo` on the DTO** rather than `Be` — comparing by shape, not reference.
- **No tests for invalid input, no `[Theory]` over status values, no exception cases** — the user didn't ask for them. Add them in a follow-up if requested.
- **Imports cover only what's used.** No `using` for libraries the file doesn't reference.

## Validation checklist

Before finishing, verify:

- [ ] File is at `src/backend/AviationApi.Tests/{MirroredFolder}/{Class}Tests.cs`.
- [ ] Namespace mirrors the SUT's namespace with `.Tests` appended (or matches sibling convention).
- [ ] Class is named `{ClassUnderTest}Tests`.
- [ ] Mocks are private readonly fields; SUT is constructed in the test class constructor.
- [ ] Method names follow `Method_Scenario_ExpectedBehavior` (or the sibling convention).
- [ ] Each test follows AAA structure with clear arrange / act / assert separation.
- [ ] Tests use `void` return types (unless testing async methods).
- [ ] Negative-path tests verify the _absence_ of side effects (`DidNotReceive`), not just the return shape.
- [ ] `ActionResult<T>` results are unwrapped via `.Result` before type-checking.
- [ ] No new NuGet packages introduced.
- [ ] Only the scenarios the user asked for are covered — no scope creep.
- [ ] No test depends on test order, environment variables, real time, or the file system unless siblings already do.

## What to avoid

- Adding NuGet packages (Moq, AutoFixture, Bogus, FsCheck, Shouldly, etc.).
- `async void` test methods. Always `async Task`.
- Mocking concrete classes — flag as a code smell instead.
- `Setup`/`Returns` chains that just re-state the SUT's logic in the test (over-specification).
- `try/catch` in tests — use `Should().Throw` / `Should().NotThrow`.
- Sleeping (`Thread.Sleep`, `Task.Delay`) for timing — unit tests should be deterministic.
- Tests that touch the real file system, network, or database — that's integration-test scope.
- Sharing mutable state via `static` fields between tests.
- One mega-test that arranges five scenarios and asserts on all of them.
- "Smoke tests" that assert nothing meaningful (`result.Should().NotBeNull()` alone isn't a behavior).
- Modifying the SUT to make it easier to test (introducing virtual methods, internal accessors) — flag the design issue instead.
- Re-asserting framework behavior (model binding, routing) — trust ASP.NET Core; test your code.

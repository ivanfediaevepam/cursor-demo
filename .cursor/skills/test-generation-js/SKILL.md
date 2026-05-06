---
name: Test Generation JS
description: Author Vitest + React Testing Library spec files for React + TypeScript components in the frontend, testing what the user sees and does — text, roles, interactions, variant styling — without testing internal state, refs, or framework behavior, and without adding new dependencies. Use whenever the user asks to write, generate, scaffold, or add tests, specs, or `.spec.tsx` / `.test.tsx` files for a React component; or mentions Vitest, React Testing Library, RTL, `screen`, `render`, `userEvent`, `findBy`, `getByRole`, "renders correctly", "covers states", or visual variants. Accepts a free-form instruction describing the SUT, scenarios, and assertions, and respects it.
---

# Test Generation JS

Write tests that **describe what a user can see and do**. The fewer the test's assumptions about internal structure (component state, refs, child component identity, DOM nesting), the slower it rots. Components get refactored; behavior shouldn't change underneath the test.

## Invocation pattern

Typically called with a free-form instruction, e.g.

> `/test-generation-js Generate a Vitest and React Testing Library spec file for FlightStatusBadge.tsx. Write tests to ensure it correctly renders the text and applies the right Tailwind classes for at least two different statuses.`

Parse it into:

1. **Target component** (path + import).
2. **Scenarios** explicitly named ("renders text", "applies classes for two statuses").
3. **Coverage scope** — exactly those scenarios, "all variants", "happy path only", etc.
4. **Interaction surface** — pure render, click handlers, form input, async.
5. **Required setup** — providers (router, theme, query client), mocked services, fake timers.

If the user names specific scenarios or a minimum count, generate exactly those — don't silently add a "renders without crashing" smoke test if it adds nothing real.

## Workflow

1. **Inspect the component.** Read the `.tsx` file. List its props, the visible output for each prop combination, the variant-style mapping (if any), interactive elements, side effects, and any context dependencies.
2. **Resolve typed prop sources.** For enum/union props, find the type definition. For variant-driven tests, that file gives both the value list and the variant map.
3. **Inspect a sibling spec.** Open one or two existing `*.spec.tsx` (or `*.test.tsx`) files. Match: filename extension, query style (role-first vs text-first), assertion style, mock-setup style, custom `render` wrappers (providers), test naming.
4. **Plan the test list** before writing. Each test answers one observable question.
5. **Write the spec** following the structure below.
6. **Verify** against the validation checklist.

## Project conventions

- **Location:** alongside the component — `src/frontend/src/components/FlightStatusBadge.spec.tsx` next to `FlightStatusBadge.tsx`.
- **Extension:** `.spec.tsx` (per the project) unless siblings use `.test.tsx` — match what exists.
- **Imports:** types/utilities from `vitest`, render utilities from `@testing-library/react`, user interactions from `@testing-library/user-event`. Don't reach for `enzyme`, `@testing-library/react-hooks`, or `react-test-renderer`.
- **No new dependencies.** Don't `npm install` anything. If a test would need MSW for HTTP mocking and MSW isn't already installed, mock the service module instead.
- **Custom `render`:** if the project has a `test-utils.tsx` or `renderWithProviders` helper, use it. Don't re-import bare `render` and skip providers.

## Test file skeleton

```tsx
import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import FlightStatusBadge from "./FlightStatusBadge";

describe("FlightStatusBadge", () => {
  it("renders …", () => {
    render(<FlightStatusBadge status="Landed" />);
    expect(screen.getByText("Landed")).toBeInTheDocument();
  });
});
```

- **One `describe` per component**, named after the component.
- **`it` over `test`** if siblings use `it`; either is fine.
- **No `beforeEach` for trivial setup** — repeat the `render` line; it's clearer than indirection.
- **`afterEach(cleanup)`** is automatic when the project has the standard Vitest + RTL config; verify before adding manual cleanup.

## RTL query priority

Use queries in this order — each next step is a fallback when the previous doesn't fit:

1. **`getByRole`** with a `name` matcher — closest to how assistive tech sees the page.
2. **`getByLabelText`** — for form fields associated with `<label>`.
3. **`getByPlaceholderText`** — only when there's no label (rare).
4. **`getByText`** — for non-interactive text content.
5. **`getByDisplayValue`** — for current values of inputs.
6. **`getByAltText` / `getByTitle`** — images and tooltips.
7. **`getByTestId`** — last resort, only when nothing above works.

Variants:

- **`getBy*`** throws if missing → use for "must exist" assertions.
- **`queryBy*`** returns `null` if missing → use for "must NOT exist" assertions.
- **`findBy*`** returns a Promise that resolves when the element appears → use for async UI.

```tsx
// Right shape for "appears after async work"
expect(await screen.findByText(/loaded/i)).toBeInTheDocument();

// Right shape for "is not present"
expect(screen.queryByText(/error/i)).not.toBeInTheDocument();
```

## userEvent patterns

```tsx
import userEvent from "@testing-library/user-event";

it("submits the form when the user clicks save", async () => {
  const onSave = vi.fn();
  const user = userEvent.setup();
  render(<EditFlightForm onSave={onSave} />);

  await user.type(screen.getByLabelText(/flight number/i), "AA123");
  await user.click(screen.getByRole("button", { name: /save/i }));

  expect(onSave).toHaveBeenCalledWith({ flightNumber: "AA123" });
});
```

- **`userEvent.setup()` once per test**, then reuse the `user` instance.
- **All `userEvent` methods are async** — always `await`.
- **Prefer `userEvent` over `fireEvent`** — `userEvent` simulates real user input including focus, keyboard events, and event ordering.

## Asserting visual variants

When the only observable difference between states is styling (a badge that changes color), asserting class names is acceptable. RTL philosophy prefers observable behavior, but if "the visible difference is the class", the class _is_ the behavior.

```tsx
expect(screen.getByText("Landed")).toHaveClass(
  "bg-green-100",
  "text-green-700",
);
```

Rules:

- **Use `toHaveClass`**, not `expect(el.className).toContain(...)`. The matcher gives better failure output and handles class lists correctly.
- **Don't snapshot** the whole `className` string — it's brittle and noisy on diff.
- **Pair class assertions with text/role assertions** — a badge that has the right color but the wrong label is still broken.
- **Don't assert on every utility class.** Pick the 1–2 that actually distinguish the variant (the background and text color, not `inline-flex items-center px-2.5`).

## Vitest mocking

```tsx
// Module mock for a service
vi.mock("../services/Flight", () => ({
  getFlights: vi.fn().mockResolvedValue([{ id: 1, status: "Landed" }]),
}));

// Spy on a function
const onClick = vi.fn();
expect(onClick).toHaveBeenCalledTimes(1);
expect(onClick).toHaveBeenCalledWith(expect.objectContaining({ id: 1 }));

// Reset between tests
beforeEach(() => vi.clearAllMocks());
```

- **`vi.mock` is hoisted** — declare it at module scope, not inside `it`.
- **For per-test mock variants**, set up the default in `vi.mock` and override with `mockReturnValueOnce` / `mockResolvedValueOnce` inside specific tests.
- **`vi.useFakeTimers()`** if testing timeouts — always pair with `vi.useRealTimers()` in `afterEach`.

## Worked example — matching the invocation

> `Ensure it correctly renders the text and applies the right Tailwind classes for at least two different statuses.`

Assumption stated inline (would be confirmed by reading `FlightStatusBadge.tsx`): the component renders the status label as visible text, and applies `bg-{color}-100 text-{color}-700` classes per status.

```tsx
import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import FlightStatusBadge from "./FlightStatusBadge";

describe("FlightStatusBadge", () => {
  it("renders the status label as text", () => {
    render(<FlightStatusBadge status="Landed" />);
    expect(screen.getByText("Landed")).toBeInTheDocument();
  });

  it("applies the green palette for Landed", () => {
    render(<FlightStatusBadge status="Landed" />);
    expect(screen.getByText("Landed")).toHaveClass(
      "bg-green-100",
      "text-green-700",
    );
  });

  it("applies the yellow palette for Delayed", () => {
    render(<FlightStatusBadge status="Delayed" />);
    expect(screen.getByText("Delayed")).toHaveClass(
      "bg-yellow-100",
      "text-yellow-800",
    );
  });

  it("forwards the className prop alongside variant classes", () => {
    render(<FlightStatusBadge status="Landed" className="ml-2" />);
    const badge = screen.getByText("Landed");
    expect(badge).toHaveClass("ml-2");
    expect(badge).toHaveClass("bg-green-100");
  });
});
```

Notes on what this does and doesn't do:

- **Two status variants tested**, as asked. Adding the rest would be a follow-up.
- **`getByText`** is the right query — the badge's only role-bearing element is the `<span>` with the label, and the user's perception is "I see the word Landed".
- **`toHaveClass`** with the 1–2 distinguishing classes per variant — not the entire base class string.
- **The fourth test verifies the `className` extension prop** — small, but a real bug surface (a regex bug in the merge could drop user classes).
- **No "renders without crashing" smoke test**, no snapshot, no full-className assertion.
- **No new dependencies, no provider wrapping, no mocks.** The component is presentational.

For a more complete suite, `it.each` is a clean way to cover every status:

```tsx
it.each([
  ["Scheduled", "bg-slate-100"],
  ["Boarding", "bg-blue-100"],
  ["Delayed", "bg-yellow-100"],
  ["InAir", "bg-indigo-100"],
  ["Landed", "bg-green-100"],
  ["Cancelled", "bg-red-100"],
] as const)("applies %s palette", (status, bgClass) => {
  render(<FlightStatusBadge status={status} />);
  expect(screen.getByText(/.+/)).toHaveClass(bgClass);
});
```

## Validation checklist

Before finishing, verify:

- [ ] File is at `{ComponentDir}/{Name}.spec.tsx` (or `.test.tsx` matching siblings), beside the component.
- [ ] One `describe` block named after the component; tests inside read as behavior sentences.
- [ ] Queries use the priority order — `getByRole` / `getByLabelText` / `getByText` over `getByTestId`.
- [ ] `getBy*` for "must exist", `queryBy*` for "must not exist", `findBy*` for async.
- [ ] All `userEvent` calls are awaited.
- [ ] Mocks are reset between tests (`vi.clearAllMocks()` in `beforeEach`, or `mockReset` in `vitest.config`).
- [ ] When asserting variant classes, the 1–2 distinguishing classes are checked — not the full class string, not a snapshot.
- [ ] Custom `render` (with providers) is used if siblings use it.
- [ ] No new npm dependencies introduced.
- [ ] Component file is unchanged.
- [ ] Only the scenarios the user asked for are covered — no scope creep.

## What to avoid

- **`getByTestId` as a first choice.** Add `data-testid` only when no role/label/text query works.
- **Testing implementation details:** state values, ref contents, child-component identity, hook internals.
- **Whole-tree snapshots** (`toMatchSnapshot()` on a rendered output). Snapshots rot fast and reviewers stop reading them.
- **`fireEvent` for high-level interactions** when `userEvent` exists — `fireEvent.click` skips focus/keyboard events that real users produce.
- **`container.querySelector('.bg-green-100')`** when `screen` queries work. The `screen` API is the supported surface.
- **Asserting on the entire `className` string** — `toHaveClass(...)` accepts a list of classes and ignores order.
- **Wrapping every test in `act()` manually.** RTL handles `act` for you; manual `act` usually means a missing `await`.
- **Testing third-party components** as if they were yours — trust their tests.
- **Time-based waits** (`setTimeout` in tests, `waitFor` with a long timeout for things that aren't actually async).
- **Mocking React, RTL, or `@testing-library/jest-dom`** — these are the test surface, not subjects.
- **Adding `enzyme`, `react-test-renderer`, `@testing-library/react-hooks`, or `msw`** if not already in `package.json`.
- **One mega-test** that renders, types, clicks, and asserts on six things. One behavior per `it`.
- **Smoke tests** like `expect(container).toBeTruthy()` that pass for any component.

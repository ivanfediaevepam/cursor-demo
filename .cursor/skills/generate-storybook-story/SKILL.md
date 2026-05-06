---
name: Generate Storybook Story
description: Author a Storybook story file (`.stories.tsx`) in Component Story Format 3 for a React + TypeScript component, exhaustively covering its meaningful states (variants, sizes, loading, error, empty, interactive) without altering the component or adding new dependencies. Use whenever the user asks to add, create, scaffold, or generate a Storybook story, `.stories.tsx`, or "stories for X"; mentions visual testing, isolation, variants, controls, autodocs, or "all states" of a component; or refers to Storybook's CSF, Meta, StoryObj, args, argTypes, decorators, or play functions. Accepts a free-form instruction describing which states to cover and respects it.
---

# Generate Storybook Story

Produce stories that **cover the component's meaningful states without overreaching into testing logic**. Each story should answer a specific visual question ("what does it look like when delayed?", "how does it behave with no data?"). Avoid one-mega-story files that try to demo everything at once — and avoid generic "Default" stubs that show nothing the component file doesn't already imply.

## Invocation pattern

Typically called with a free-form instruction, e.g.

> `/generate-storybook-story Generate a complete Storybook file for this component. I want a story for every single possible FlightStatus so we can visually test all the color variations in isolation.`

Parse it into:

1. **Target component** (path + import name).
2. **Coverage scope** — explicit story list, "every variant", "all states", "happy path only", etc.
3. **Driving prop(s)** — which prop varies across stories.
4. **Source of variant values** — enum, union type, hardcoded list. If an enum/union, locate it before writing.
5. **Special needs** — interaction tests (`play`), decorators (theme/router/providers), mocked data, custom layout.

If the instruction says "every possible value" of a typed prop, the value list is not a guess — read the type definition and enumerate exhaustively.

## Workflow

1. **Inspect the component.** Read its `.tsx` file. Note the props interface, the prop types' source files, defaults, and any required context (theme provider, router, query client).
2. **Resolve variant sources.** For each variant prop, find its type definition. Enum/union members become the story list when the user asks for full coverage.
3. **Inspect existing stories.** Open one or two `*.stories.tsx` files in the project. Match: the `title` hierarchy ("Components/X" vs "Flights/X"), `tags` (e.g., `['autodocs']`), `parameters` (layout, backgrounds), decorator usage, naming style.
4. **Pick story granularity.** Default rule: one story per visually distinct state. Add a single "Gallery" story only when comparing variants side-by-side has clear value (e.g., color palettes, size scales).
5. **Write the file** using CSF 3 (`Meta` + `StoryObj`) — see the structure below.
6. **Verify** against the validation checklist.

## File placement and naming

- **Location:** next to the component — `src/frontend/src/components/FlightStatusBadge.stories.tsx` lives beside `FlightStatusBadge.tsx`.
- **Filename:** `{ComponentName}.stories.tsx`, PascalCase, matching the component exactly.
- **Title hierarchy:** `Components/{ComponentName}` by default; match what siblings use if they group by feature (`Flights/StatusBadge`).

## CSF 3 structure

Every story file Claude generates uses this skeleton:

```tsx
import type { Meta, StoryObj } from "@storybook/react";
import FlightStatusBadge from "./FlightStatusBadge";

const meta = {
  title: "Components/FlightStatusBadge",
  component: FlightStatusBadge,
  tags: ["autodocs"],
  parameters: {
    layout: "centered",
  },
  argTypes: {
    // controls + descriptions for props
  },
} satisfies Meta<typeof FlightStatusBadge>;

export default meta;
type Story = StoryObj<typeof meta>;

// Stories below as named exports.
```

Rules:

- **`satisfies Meta<typeof Component>`** preserves prop types in stories (so `args` is type-checked against the component's props).
- **Stories are objects, not functions.** `export const X: Story = { args: { ... } };` — never `export const X = (args) => <Component {...args} />` (CSF 2).
- **Named exports become stories.** Names are PascalCase; Storybook auto-spaces them in the sidebar.
- **`tags: ['autodocs']`** generates a Docs page from JSDoc and prop types — include unless the project disables autodocs project-wide.
- **`parameters.layout`:** `'centered'` for small components (badges, buttons, inputs), `'padded'` for medium (cards, forms), `'fullscreen'` for pages.
- **No new dependencies.** Don't reach for `@storybook/test`, `msw`, `@storybook/addon-themes`, etc. unless already in `package.json`.

## Story patterns

### One per variant value (enum/union driven)

When a prop's type is an enum or union and the user wants full coverage, emit one story per member. Pull the values from the type definition — don't paraphrase from memory.

```tsx
export const Scheduled: Story = { args: { status: "Scheduled" } };
export const Boarding: Story = { args: { status: "Boarding" } };
// ...
```

### Default + edge cases (state-driven)

When the prop space isn't enumerable (data, callbacks, content), pick states by behavior:

```tsx
export const Default: Story = { args: { items: mockItems } };
export const Loading: Story = { args: { items: [], isLoading: true } };
export const Empty: Story = { args: { items: [] } };
export const Error: Story = { args: { items: [], error: "Failed to load" } };
```

### Gallery (comparison view)

A single story that renders every variant together — useful for color palettes and size scales, not a substitute for individual stories.

```tsx
export const Gallery: Story = {
  parameters: { layout: "padded" },
  render: () => (
    <div className="flex flex-col gap-2">
      {STATUSES.map((s) => (
        <div key={s} className="flex items-center gap-3">
          <FlightStatusBadge status={s} />
          <span className="text-sm text-slate-600">{s}</span>
        </div>
      ))}
    </div>
  ),
};
```

### Interaction (play function) — only when asked

```tsx
import { userEvent, within, expect } from "@storybook/test";

export const ClickToOpen: Story = {
  args: {
    /* ... */
  },
  play: async ({ canvasElement }) => {
    const canvas = within(canvasElement);
    await userEvent.click(canvas.getByRole("button", { name: /open/i }));
    await expect(canvas.getByRole("dialog")).toBeVisible();
  },
};
```

Only include `play` when `@storybook/test` is already a dependency and the user asked for interaction coverage. Don't introduce it speculatively.

## Worked example — matching the invocation

> `Generate a complete Storybook file… a story for every single possible FlightStatus.`

Assumption stated inline: `FlightStatus` is `'Scheduled' | 'Boarding' | 'Delayed' | 'InAir' | 'Landed' | 'Cancelled'` (read from `src/frontend/src/services/flights.ts` — adjust if the actual definition differs).

```tsx
import type { Meta, StoryObj } from "@storybook/react";
import FlightStatusBadge from "./FlightStatusBadge";
import type { FlightStatus } from "../services/Flight";

const STATUSES: FlightStatus[] = [
  "Scheduled",
  "Boarding",
  "Delayed",
  "InAir",
  "Landed",
  "Cancelled",
];

const meta = {
  title: "Components/FlightStatusBadge",
  component: FlightStatusBadge,
  tags: ["autodocs"],
  parameters: {
    layout: "centered",
  },
  argTypes: {
    status: {
      control: "select",
      options: STATUSES,
      description: "The flight status the badge represents.",
    },
    className: {
      control: "text",
      description: "Extra Tailwind classes appended to the badge.",
    },
  },
} satisfies Meta<typeof FlightStatusBadge>;

export default meta;
type Story = StoryObj<typeof meta>;

// One story per status — direct visual coverage of every variant.
export const Scheduled: Story = { args: { status: "Scheduled" } };
export const Boarding: Story = { args: { status: "Boarding" } };
export const Delayed: Story = { args: { status: "Delayed" } };
export const InAir: Story = { args: { status: "InAir" } };
export const Landed: Story = { args: { status: "Landed" } };
export const Cancelled: Story = { args: { status: "Cancelled" } };

// Side-by-side gallery for palette comparison.
export const AllStatuses: Story = {
  parameters: { layout: "padded" },
  render: () => (
    <div className="flex flex-col gap-2">
      {STATUSES.map((s) => (
        <div key={s} className="flex items-center gap-3">
          <FlightStatusBadge status={s} />
          <span className="text-sm text-slate-600">{s}</span>
        </div>
      ))}
    </div>
  ),
};
```

Notes on what this does and doesn't do:

- **One named export per `FlightStatus` value** — the user asked for "in isolation", so each variant gets its own sidebar entry rather than being buried in a grid.
- **Plus a `Gallery` story** for comparison — useful but not a replacement for the individuals.
- **`STATUSES` array is typed and reused** as both the story enumeration and the `argTypes.options` list. If a new status is added to the union and forgotten here, TypeScript surfaces the gap.
- **No `play` function, no decorators, no mocks** — the component is presentational and needs none.
- **No new dependencies.**

If Storybook's static analysis ever loosens to allow programmatic exports, the per-status block could collapse into a loop. As of CSF 3, story exports must be statically analyzable, so the explicit list is the correct shape.

## argTypes and parameters reference

| Need                        | Configuration                                              |
| --------------------------- | ---------------------------------------------------------- |
| Dropdown control            | `control: 'select', options: [...]`                        |
| Radio buttons               | `control: 'radio', options: [...]`                         |
| Boolean toggle              | `control: 'boolean'`                                       |
| Number slider               | `control: { type: 'range', min: 0, max: 100, step: 1 }`    |
| Color picker                | `control: 'color'`                                         |
| Hide a prop                 | `table: { disable: true }`                                 |
| Action logger for callbacks | (callback props are auto-detected if named `on*`)          |
| Centered small components   | `parameters: { layout: 'centered' }`                       |
| Full-bleed page             | `parameters: { layout: 'fullscreen' }`                     |
| Custom backgrounds          | `parameters: { backgrounds: { default: 'dark' } }`         |
| Viewport for responsive     | `parameters: { viewport: { defaultViewport: 'mobile1' } }` |

## Validation checklist

Before finishing, verify:

- [ ] File is at `{ComponentDir}/{ComponentName}.stories.tsx`, beside the component.
- [ ] Imports `Meta` and `StoryObj` as types from `@storybook/react`.
- [ ] Uses CSF 3: stories are objects, not function components.
- [ ] `meta` ends with `satisfies Meta<typeof Component>` — preserves prop type-checking.
- [ ] `title` matches the project's hierarchy convention.
- [ ] `tags` and `parameters.layout` match siblings.
- [ ] When the user asked for "every variant" of a typed prop, the type was actually consulted and every member has a story.
- [ ] Variant-driving array (e.g., `STATUSES`) is typed against the union/enum so missing members are caught at compile time.
- [ ] No `play`, decorators, or mocks introduced unless asked or already in use elsewhere.
- [ ] No new npm dependencies.
- [ ] Component file is unchanged.

## What to avoid

- **CSF 2 syntax** — `export const X = (args) => <Component {...args} />`. Use `StoryObj` objects.
- **Inline mock data of types you can import** — pull factories or fixtures from `src/frontend/src/services/` or `__fixtures__/` if they exist.
- **A single mega-story** that switches mode based on a knob. Use named exports, one state each.
- **Generic placeholder stories** like `Default` with no `args` when a meaningful default is obvious from the component.
- **Hardcoded enum/union values** retyped from memory instead of imported.
- **Adding `@storybook/test`, `msw`, theming addons, etc.** when not already in `package.json`.
- **Modifying the component** to make it easier to story (extracting state, exposing internals). The component is read-only here.
- **Decorators that change the component's rendered output** (forced wrappers, fixed widths) without saying so. If a decorator is needed for context (router, query client), include only the minimum.
- **Skipping `tags: ['autodocs']`** when siblings use it — autodocs is the cheapest documentation in the project.
- **Story names that conflict with reserved exports** (`default`) or aren't valid JS identifiers.

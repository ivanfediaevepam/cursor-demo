---
name: Generate React Component
description: Scaffold a new React + TypeScript + Tailwind component in the frontend that matches existing project conventions — file location, prop typing, default export, styling idioms, accessibility, and no new npm dependencies. Use whenever the user asks to add, create, scaffold, or build a React component, badge, card, list, modal, form, page, layout, or any `.tsx` file. Triggers include "new component", "add a `<X />`", "create … component", a `.tsx` filename, or any UI element described in component terms ("a status pill", "a sortable table"). Accepts a free-form instruction describing the component's name, props, behavior, and visual reference, and respects it.
---

# Generate React Component

Generate a component that **looks like the rest of the codebase wrote it**. Match the existing typing style, the existing class-composition idiom, the existing import order, and the existing accessibility patterns — even when those differ from what's currently fashionable in the wider React community. Project consistency beats external best practice.

## Invocation pattern

Typically called with a free-form instruction, e.g.

> `/generate-react-component Create a new FlightStatusBadge.tsx component. It should take a status prop (using the FlightStatus enum). Render a styled pill badge where the background color changes based on the status (e.g., green for Landed, yellow for Delayed). Match the Tailwind aesthetic of PlaneList.`

Parse it into:

1. **Component name and file** (`FlightStatusBadge` → `src/frontend/src/components/FlightStatusBadge.tsx`).
2. **Props** (name, type, optionality — pulled from existing types when possible).
3. **Behavior** (stateless render, internal state, side effects, callbacks).
4. **Visual reference** (named sibling component to mirror).
5. **Variants** (the color-by-status mapping in the example above).

If a field is genuinely ambiguous and the project's existing patterns don't resolve it, ask one question. Otherwise pick the convention seen in siblings and state the assumption inline.

## Workflow

1. **Inspect before writing.** Open the named visual reference (here, `PlaneList.tsx`) and at least one other component in `src/frontend/src/components/`. Note: prop typing style, export style, class-composition idiom (`clsx`, `cn`, template literals, plain string concatenation), Tailwind palette in use, spacing scale, font sizes, hover/focus rings, dark-mode handling, accessibility attributes, file-internal helper placement.
2. **Locate the relevant types.** For typed enums or domain types referenced in props, look in `src/frontend/src/services/` (or wherever the project keeps shared types). Import — don't redeclare.
3. **Pick the component shape.** Pure function returning JSX. Stateless unless the instruction implies state. Hooks only when needed.
4. **Compose styles** using whatever idiom the siblings use. Don't introduce a new one (no `clsx` if siblings use template literals; no `cva` if it isn't already a dependency).
5. **Write the component.**
6. **Verify** against the validation checklist.

## Project conventions

- **Location:** `src/frontend/src/components/{ComponentName}.tsx`. One component per file. Filename in PascalCase, matching the component name exactly.
- **Export:** `export default ComponentName;` at the bottom. Named export only if the project pattern is named exports — verify.
- **Prop typing:** explicit `interface {ComponentName}Props { ... }` above the component. Use `React.FC<Props>` if siblings do; switch to a plain function signature `function ComponentName(props: Props): JSX.Element` only if siblings do.
- **No `any`.** Use the project's existing types from `src/frontend/src/services/` (or wherever they live). For unknown shapes, prefer `unknown` + narrowing over `any`.
- **No new dependencies.** Don't `npm install` anything. If you'd reach for `clsx`, `classnames`, or `cva`, fall back to the idiom siblings already use.
- **No CSS files.** Tailwind utility classes only. If you'd write a `style={{ ... }}` prop, justify it (genuinely dynamic values like a percentage from a prop are fine; static values aren't).
- **Imports order:** external libs (`react`, third-party) → internal types/services → sibling components → assets. Match what siblings do.

## TypeScript patterns

```tsx
// Props above, default-exported component below.
interface FlightStatusBadgeProps {
  status: FlightStatus;
  className?: string; // optional extension hook for callers
}

const FlightStatusBadge: React.FC<FlightStatusBadgeProps> = ({
  status,
  className,
}) => {
  // ...
};

export default FlightStatusBadge;
```

- **Optional props** get `?` and a default in destructuring (`{ size = 'md' }`).
- **Children:** if the component takes children, type explicitly with `children: React.ReactNode` (React 18 dropped implicit children from `FC`).
- **Variant props:** prefer string-literal unions (`'sm' | 'md' | 'lg'`) over booleans, and over enums when the values are component-local.
- **Discriminated unions** for mutually exclusive prop combinations (`{ kind: 'link'; href: string } | { kind: 'button'; onClick: () => void }`).
- **Event handler types** are explicit: `onClick: (e: React.MouseEvent<HTMLButtonElement>) => void`.

## Tailwind styling patterns

The most useful pattern for variant-driven components is a **lookup map**, which avoids both `any` and added dependencies:

```tsx
const statusStyles: Record<FlightStatus, string> = {
  Scheduled: "bg-slate-100 text-slate-700 ring-slate-200",
  Boarding: "bg-blue-100 text-blue-700 ring-blue-200",
  Delayed: "bg-yellow-100 text-yellow-800 ring-yellow-200",
  InAir: "bg-indigo-100 text-indigo-700 ring-indigo-200",
  Landed: "bg-green-100 text-green-700 ring-green-200",
  Cancelled: "bg-red-100 text-red-700 ring-red-200",
};
```

Rules:

- **`Record<EnumType, string>` makes the map exhaustive** — TypeScript will complain if a new enum value is added and forgotten here. Don't use a plain object literal that loses this guarantee.
- **Compose classes the same way siblings do.** If `PlaneList.tsx` uses template literals, use template literals. If it imports a `cn`/`clsx` util, import the same one.
- **Use Tailwind's design tokens consistently** — don't mix `bg-green-500` with `bg-[#22c55e]` in one project. Read what siblings use.
- **Size/spacing scale:** match siblings (e.g., if all badges in the project use `px-2.5 py-0.5 text-xs`, do the same).
- **Hover/focus states** belong on interactive elements only. Don't add `hover:` classes to a static badge.
- **Dark mode:** if siblings have `dark:` variants, include them; if they don't, don't introduce them.

## Worked example — matching the invocation

Assumptions stated inline (would be confirmed by reading `PlaneList.tsx`): `FlightStatus` is exported from `src/frontend/src/services/`; siblings use `React.FC<Props>` and template-literal class composition; badges use the `bg-*-100 / text-*-700 / ring-*-200` palette with `px-2.5 py-0.5 text-xs font-medium`.

```tsx
import React from "react";
import { FlightStatus } from "../services/Flight";

interface FlightStatusBadgeProps {
  status: FlightStatus;
  className?: string;
}

const statusStyles: Record<FlightStatus, string> = {
  Scheduled: "bg-slate-100 text-slate-700 ring-slate-200",
  Boarding: "bg-blue-100 text-blue-700 ring-blue-200",
  Delayed: "bg-yellow-100 text-yellow-800 ring-yellow-200",
  InAir: "bg-indigo-100 text-indigo-700 ring-indigo-200",
  Landed: "bg-green-100 text-green-700 ring-green-200",
  Cancelled: "bg-red-100 text-red-700 ring-red-200",
};

const statusLabels: Record<FlightStatus, string> = {
  Scheduled: "Scheduled",
  Boarding: "Boarding",
  Delayed: "Delayed",
  InAir: "In air",
  Landed: "Landed",
  Cancelled: "Cancelled",
};

const FlightStatusBadge: React.FC<FlightStatusBadgeProps> = ({
  status,
  className,
}) => {
  const base =
    "inline-flex items-center px-2.5 py-0.5 text-xs font-medium rounded-full ring-1 ring-inset";
  return (
    <span
      className={`${base} ${statusStyles[status]} ${className ?? ""}`}
      aria-label={`Flight status: ${statusLabels[status]}`}
    >
      {statusLabels[status]}
    </span>
  );
};

export default FlightStatusBadge;
```

Notes on what this does and doesn't do:

- **Imports `FlightStatus`** rather than redeclaring. If the actual import path differs, that's a one-line correction at the top.
- **Two `Record<FlightStatus, …>` maps** — one for styling, one for human-readable labels. Both become exhaustive checks.
- **`aria-label` includes the status text** — color alone never carries meaning for accessibility. Since the visible text already says the status, this is partially redundant but explicit; remove the `aria-label` if siblings don't add one.
- **`className` extension prop** lets callers add layout classes (`mt-2`, `ml-auto`) without forking the component.
- **No state, no effects, no callbacks** — the instruction described a pure presentation component, so the implementation is one.
- **No new dependencies, no CSS file, no inline styles.**

## Accessibility checklist

For any component that renders interactive or status-bearing UI:

- Use the **semantically correct element** (`<button>` for actions, `<a>` for navigation, `<span>` for inline text, `<output>` or `role="status"` for live values).
- **Color is never the only signal** — pair every color-coded variant with text or an icon.
- **Interactive elements** have visible focus states (not removed by `outline-none` without a replacement) and reachable by keyboard.
- **Labels:** images have `alt`; icon-only buttons have `aria-label`; form inputs have associated `<label>` elements.
- **Live regions** (`role="status"`, `aria-live`) on content that updates without user interaction.

## Validation checklist

Before finishing, verify:

- [ ] File is at `src/frontend/src/components/{Name}.tsx` with PascalCase filename matching the component.
- [ ] One component per file. Default-exported (or named, matching project convention).
- [ ] Props are an explicit `interface`; component uses `React.FC<Props>` if siblings do.
- [ ] No `any`. Domain types imported from `src/frontend/src/services/` rather than redeclared.
- [ ] Variant-driven styling uses `Record<UnionType, string>` so the compiler enforces exhaustiveness.
- [ ] Class-composition idiom matches siblings (template literals vs `cn`/`clsx`).
- [ ] Tailwind palette, spacing, and font-size tokens match siblings.
- [ ] No `style={{ ... }}` for static values.
- [ ] `className?: string` extension prop included if siblings expose one.
- [ ] Accessibility: semantic element, color is not the only signal, labels where needed.
- [ ] No new npm dependencies introduced.
- [ ] Imports ordered consistently with siblings.
- [ ] No `console.log`, no commented-out code, no `TODO` left in the final file.

## What to avoid

- `any`, `as any`, `@ts-ignore`, `@ts-expect-error` without a reason comment.
- Importing or installing `clsx`, `classnames`, `cva`, `tailwind-merge`, `react-icons`, etc. unless already in `package.json`.
- Inline `style={{ ... }}` for values expressible as Tailwind classes.
- A new CSS or `.module.css` file.
- `useState` for values derivable from props.
- `useEffect` for synchronization that should be derived state.
- Mutating props or shared objects.
- Non-semantic `<div>` where a `<button>`, `<a>`, `<span>`, `<output>`, or heading would be correct.
- `outline-none` / `focus:outline-none` without a replacement focus indicator.
- Hardcoding hex colors when the project uses Tailwind palette tokens.
- Inventing prop names that conflict with HTML attributes (e.g. a custom `type` on a button without merging with native types).
- Default-exporting an anonymous arrow function (`export default (props) => …`) — name the component for stack traces and devtools.
- Adding tests, stories, or docs files unless asked.

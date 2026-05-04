# Storybook MCP — Demo Script

This file is the **live demo cheat-sheet** for showcasing the Storybook MCP
integration. Run it step-by-step in Cursor (or any MCP-enabled agent) while
Storybook is running in a terminal.

---

## Setup (before the demo)

**Terminal 1 — start Storybook:**
```sh
cd src/frontend
npm run storybook
```

Open **http://localhost:6006** to confirm Storybook is running.
Open **http://localhost:6006/mcp** to confirm the MCP server is live.

**In Cursor:**
- Open Settings → MCP → confirm `aviation-demo-sb-mcp` shows **Connected**
- Open a new Agent chat (⌘+I)

---

## Step 1 — Discovery (30 seconds)

> **Show**: the agent knows about all your components without you telling it anything.

**Prompt:**
```
Using the Storybook MCP, list all documented components in this project.
```

**Expected**: Agent calls `list-all-documentation` → returns a table of
`Banner`, `Card`, `PlaneList`, `PlaneSpinner`, `FlightDetails` with their
story counts.

---

## Step 2 — Component Deep-Dive (60 seconds)

> **Show**: the agent reads actual prop types and usage — no hallucination.

**Prompt:**
```
Using the Storybook MCP, tell me everything about the PlaneList component —
its props, accepted values, and available stories.
```

**Expected**: Agent calls `get-documentation` for `PlaneList` → returns the
props table (`planes?: Plane[]`), descriptions for all four stories
(Default, SinglePlane, EmptyList, ManyPlanes), and the JSDoc component
description.

---

## Step 3 — Inline Story Preview (60 seconds)

> **Show**: rendered UI appears directly inside the chat, no browser switching needed.

**Prompt:**
```
Show me a preview of all PlaneList stories using the Storybook MCP.
```

**Expected**: Agent calls `preview-stories` → stories are rendered as live
previews embedded in the Cursor chat panel (requires MCP Apps support in
agent).

---

## Step 4 — Run Tests with Self-Healing Loop (90 seconds)

> **Show**: the agent runs tests, finds issues, fixes them, and re-validates.

**Prompt:**
```
Run the Storybook tests for all PlaneSpinner stories using the Storybook MCP.
Report any failures and fix them if found.
```

**Expected**:
1. Agent calls `run-story-tests` for the PlaneSpinner stories
2. If a11y or interaction tests fail, the agent reads the error, edits the code, and re-runs
3. Final output: all tests pass ✅

---

## Step 5 — Component Generation Using the Design System (2 minutes)

> **Show**: the "wow" moment — the agent builds a new component that looks
> native to the project because it consulted the docs first.

**Prompt:**
```
Using the Storybook MCP to understand our existing components and design
system, create a new StatusBadge component for flight statuses (Scheduled,
In Flight, Completed, Cancelled). It should match the amber colour palette
from our existing components. Then write stories for all four statuses and
run the tests.
```

**Expected sequence**:
1. `list-all-documentation` → discovers existing components
2. `get-documentation` for `Card` and `FlightDetails` → reads amber palette + text styles
3. Creates `src/components/StatusBadge.tsx` using the same Tailwind colour tokens
4. Creates `src/components/StatusBadge.stories.tsx` with four status stories + autodocs
5. `run-story-tests` → all pass ✅

---

## Key Takeaways

| Without Storybook MCP | With Storybook MCP |
|---|---|
| Agent guesses prop names | Agent reads actual prop types from manifests |
| Agent uses generic styling | Agent reuses documented colour tokens |
| Developer manually runs tests | Agent runs tests and self-heals |
| No UI preview in chat | Stories rendered inline in agent chat |
| Context window bloat | Structured, on-demand component knowledge |

---

## MCP Tools Reference

| Tool | Toolset | What it does |
|---|---|---|
| `list-all-documentation` | Docs | Lists all documented components |
| `get-documentation` | Docs | Returns props + stories for a component |
| `get-documentation-for-story` | Docs | Returns full source for a specific story |
| `get-storybook-story-instructions` | Development | Returns current story-writing conventions |
| `preview-stories` | Development | Renders stories inline in agent chat |
| `run-story-tests` | Testing | Runs interaction + a11y tests, returns results |

# Demo Cheat Sheet

## DEMO 1 — Three Modes, Same Problem
**State:** `HomePage.tsx` open. Notice the planes list is hardcoded inline.
**Action:** Select the `planes` array.
**Mode:** Ask
**Prompt:**
> Why is this list hardcoded when there's already a PlaneService that fetches planes?

**Action:** Select the `planes` array.
**Mode:** Cmd+K (Edit)
**Prompt:**
> Sort this array by year descending.

**Mode:** Agent
**Prompt:**
> HomePage hardcodes the planes list. Wire it up to PlaneService.getPlanes() using TanStack Query, with loading and error states. Don't change the visual layout.

---

## DEMO 2 — Agentic Loop on a Real Bug
**State:** `FlightRepository.cs` open. Look at `GetFlightById` (uses `ElementAt(id)`).
**Mode:** Agent
**Prompt:**
> GET /flights/2 returns the flight with ID 3 instead of ID 2. Find the bug, fix it, and run the tests to confirm the fix works.

**Follow-up Prompt (if agent misses regression test):**
> Add a regression test that would have caught this bug.

---

## DEMO 3 — Plan Mode, Multi-File Feature
**State:** `stories/Plan.md` open.
**Mode:** Plan (Shift+Tab)
**Prompt:** (Paste the user story)
> As a user,
> I want to be able to update a plane in the project,
> so that I can keep the information up to date.
> 
> Acceptance Criteria:
> - The user can update the plane's name, description.
> - The date and time of the last update should be recorded and displayed.
> - The system should validate the inputs to ensure data integrity.
> - The user should receive a confirmation message upon successful update.

**Action:** Answer any clarifying questions asked by the agent.
**Action:** Edit the generated plan live. Add constraints:
- "Do not change the in-memory storage approach."
- "Validate name is non-empty and ≤ 100 characters."
**Action:** Click Save to workspace -> Click Build.

---

## DEMO 4 — Debug Mode, The Crashed Plane
**State:** Backend on port 1903, frontend on 5173.
**Action:** Open UI, click any plane. Watch the animation crash.
**Mode:** Debug
**Prompt:**
> When I click any plane on the home page and the detail view loads, an animation plays showing a crash. I expected the plane to land. Help me figure out why.

**Action:** Reproduce the issue in the browser while the agent is listening.
**Action:** Agent fixes the backend. Click "Mark Fixed".

---

## DEMO 5 — Terminal + Diff Review
**Mode:** Agent
**Prompt:**
> Add a `RangeInKm` field to the Plane domain end-to-end: backend model, controller responses, frontend Plane type, and update PlaneDetail.tsx to show it. Run the backend tests. Don't add new dependencies.

**Action:** Approve terminal commands. Review diffs. Find the weakened test assertion.
**Action:** Reject the test hunk.
**Follow-up Prompt:**
> Use the existing pattern from PlanesControllerTests.cs for the new test, and seed realistic ranges for the existing planes.

---

## DEMO 6 — Skills, Encoding the Team's Playbook
**State:** Diff from Demo 5 still in workspace. Open `.cursor/skills/aviation-pr-review/SKILL.md`.
**Mode:** Agent
**Action:** Type `/` and select the PR Review skill from the picker.
**Action:** Run it. Review the risk-graded summary and checklist output.

---

## DEMO 7 — Pitfalls Live
### Confident Hallucination
**State:** `FlightService.ts` open.
**Mode:** Agent
**Prompt 1:**
> Refactor FlightService.ts to use modern axios patterns with interceptors and proper error handling.

**Action:** Watch it invent outdated patterns. Stop and Restore Checkpoint.
**Prompt 1 (Corrected):**
> Refactor @FlightService.ts using axios 1.x patterns. use context7

### The Doom Loop
**State:** Break a test intentionally. Cmd+K on `PlanesControllerTests.cs`, change `HaveCount(3)` to `HaveCount(5)`.
**Prompt 2 (Vague):**
> Fix the failing tests.

**Action:** Watch the agent flail. Stop the loop. Restore Checkpoint.
**Prompt 2 (Corrected):**
> PlanesControllerTests.GetAll_ReturnsListOfPlanes is failing because the expected count is wrong. The seed data has 3 planes. Fix only the assertion.
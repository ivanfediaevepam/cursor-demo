# plan-implement-review

You are running the **planner → developer → reviewer orchestration** for a **complex goal**, a **backlog-style list of tasks**, or **multi-step work** that should not be done in one inline pass unless the user explicitly overrides this command.

## Required first step

Read and follow the full workflow in:

`.cursor/skills/subagent-plan-develop-review-orchestration/SKILL.md`

That skill is authoritative for phase order, per-subtask loops, progression rules, and what to do when subagents cannot be spawned.

## What you must do (summary)

1. **Capture intent** — Before planning, restate scope, non-goals, dependencies (e.g. no new packages), and test expectations so the planner is not guessing.

2. **Planner subagent** — Spawn using configuration from `.cursor/agents/planner.md`. Pass the full user goal and constraints. The planner produces `plan.md` and ordered `NN-*.md` files under `.cursor/tasks/<plan-slug>/` per that agent’s rules. Wait for completion; resolve any blockers before executing subtask `01`.

3. **For each subtask in plan order** (as linked from `plan.md`):
   - **Developer subagent** — Spawn using `.cursor/agents/developer.md` with **one** subtask file as primary scope (objective, acceptance criteria, verification, plus `plan.md` for context).
   - **Code-quality-reviewer subagent** — After the developer finishes, spawn using `.cursor/agents/code-quality-reviewer.md` on that subtask’s change set.
   - **Iterate developer ↔ reviewer** for that subtask until there are **no Critical items** and **no Warnings that block advancing** (per the orchestration skill). Re-run the reviewer after substantive fixes.

4. **Advance** to the next `NN-*.md` only when the current subtask meets acceptance criteria and the reviewer gate is satisfied. Do not skip numbered subtasks unless the user rescopes or cancels them.

5. **Deeper micro-loop detail** (developer ↔ reviewer handoffs, nit policy) — When needed, align with `.cursor/skills/subagent-dev-review-workflow/SKILL.md`.

Use the same subagent mechanism this session supports (for example the Task tool with the appropriate subagent type, or project-specific `@` agent references). If subagents cannot be spawned, state that honestly and offer the closest inline alternative while still following plan file order.

When the pipeline is complete, summarize for the user: plan location, what shipped per subtask, reviewer outcomes, and any remaining non-blocking suggestions.

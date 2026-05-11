---
name: subagent-plan-develop-review-orchestration
description: Orchestrates large multi-step work by running the planner subagent first (plan plus `.cursor/tasks/` files), then for each subtask file running the developer subagent and the code-quality-reviewer subagent in a loop until the subtask is done, advancing until every planned task is complete. Use when the user delivers a large complex goal, a backlog-style list of tasks, multi-area work (backend plus frontend plus tests), unclear sequencing, or asks for planner-led execution with review gates between tasks.
disable-model-invocation: true
---

# Planner → developer → reviewer orchestration

When the parent agent receives a **large complex task**, a **list of tasks**, or work that clearly spans **multiple ordered steps**, it must **not** collapse everything into a single inline pass. It runs the **subagent sequence** below until **all** planned subtasks are completed.

## Agent definitions (launch in this order for the overall workflow)

| Phase | Role | Configure from |
|-------|------|----------------|
| Planning | Planner | [.cursor/agents/planner.md](../../agents/planner.md) |
| Per subtask | Developer | [.cursor/agents/developer.md](../../agents/developer.md) |
| After each developer pass on a subtask | Code quality reviewer | [.cursor/agents/code-quality-reviewer.md](../../agents/code-quality-reviewer.md) |

Use the same mechanism the product uses to spawn subagents (for example the **Task** tool with the appropriate subagent type, or an `@` reference to the agent file). Each spawned agent must receive a **self-contained brief**: goal, constraints, paths to `plan.md` and the current `NN-*.md` task file when executing a subtask.

## Mandatory overall sequence

1. **Invoke the planner subagent first** — Parent passes the full user goal, constraints, and any clarifications. The planner **restates scope**, **decomposes** into ordered subtasks, and **writes files** under `.cursor/tasks/<plan-slug>/` per that agent’s rules (`plan.md`, `01-….md`, `02-….md`, …). Do not skip file creation when the environment allows writes.
2. **Wait until the planner finishes** — Obtain `plan.md` path, subtask count, execution order, and any **blockers** the planner flagged. If the user must decide something before `01`, pause the pipeline and resume only after resolution.
3. **For each subtask file in plan order** (typically `01`, `02`, … as linked from `plan.md`):
   - **Invoke the developer subagent** with that **single** subtask file as the primary scope (objective, acceptance criteria, verification from the task file, plus link back to `plan.md` for context).
   - **After the developer finishes**, **invoke the code-quality-reviewer subagent** on the change set the developer produced for that subtask (diff, touched files, tests).
   - **Iterate developer ↔ reviewer for that subtask** until the reviewer’s verdict has **no Critical items** and **no Warnings that block advancing** (treat material gaps the same as Warnings). Address findings by launching the developer again with a structured list of reviewer items; re-run the reviewer after substantive fixes. If the reviewer approves with **only** non-blocking suggestions, move on unless the user asked to implement those nits now.
4. **Advance to the next subtask** only when the current one meets its acceptance criteria and the reviewer gate above is satisfied.
5. **Stop when every** `NN-*.md` subtask in the plan has completed this cycle.

## Loop discipline (per subtask)

- **One subtask per developer invocation** when possible so diffs stay reviewable; do not batch unrelated task files into one developer call unless the plan explicitly merges them.
- After **Critical** fixes or a **large** follow-up diff, run the code-quality-reviewer **again** on that subtask before advancing.
- For deeper detail on the **developer → reviewer → developer** micro-loop (handoff format, nit policy), see [subagent-dev-review-workflow](../subagent-dev-review-workflow/SKILL.md).

## Parent agent responsibilities

- **Before planning:** Capture the user’s full intent, non-goals, dependency rules (e.g. no new packages), and test expectations so the planner does not guess wrong.
- **Between phases:** After each subagent returns, read its summary and artifacts (paths, commands, blockers) before spawning the next.
- **Task progression:** Track which `NN-*.md` files are done; never skip a numbered subtask that the plan still lists unless the user explicitly cancels or rescopes it.
- **Honesty on tooling:** If subagents cannot be spawned in the current session, state that limitation and offer the closest inline alternative (still following plan file order).

## Optional progress checklist

Copy and tick in the parent thread as work proceeds:

```
Orchestration progress:
- [ ] Planner invoked; plan.md and all NN-*.md files exist
- [ ] Blockers from planner resolved (if any)
- [ ] Subtask 01 — developer → reviewer → … → done
- [ ] Subtask 02 — …
- [ ] (repeat per plan)
- [ ] All subtasks complete
```

## What this skill does not require

- It does **not** replace individual agent files; those define how each role behaves.
- It does **not** force a final repo-wide review after the last subtask unless the user asks; the last subtask’s reviewer pass is the default gate.

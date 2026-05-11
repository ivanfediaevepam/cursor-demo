---
name: subagent-dev-review-workflow
description: Runs implementation via the developer subagent, then quality review via the code-quality-reviewer subagent, then sends the developer back to address findings. Use when the user assigns implementation, creation, scaffolding, wiring, feature work, refactors for behavior, or any substantive development task that should be executed by subagents rather than inline in one shot.
disable-model-invocation: true
---

# Subagent dev → review → dev workflow

When the parent agent receives a task for **implementation**, **creation**, or any other **development** work, it must **not** do the full implementation inline by default. It orchestrates this sequence instead.

## Agent definitions

| Role | Launch via |
|------|------------|
| Worker (implementation) | Subagent configured from [.cursor/agents/developer.md](../../agents/developer.md) |
| Code reviewer | Subagent configured from [.cursor/agents/code-quality-reviewer.md](../../agents/code-quality-reviewer.md) |

Use the same mechanism the product uses to spawn subagents (for example the **Task** tool with the appropriate subagent type, or an `@` reference to the agent file if that is how subagents are selected in the session). Point the spawned agent at the **full task context** (requirements, constraints, files in scope) so the worker does not start blind.

## Mandatory sequence

1. **Launch the developer subagent first** — it performs the implementation work to completion (code, tests if in scope, verification commands as that agent defines).
2. **After the developer finishes**, **launch the code-quality-reviewer subagent** — it evaluates the same change set the developer produced, following its checklist and output format (Summary, Critical, Warnings, Suggestions, Positive notes).
3. **If the reviewer reports** any of the following, it must **spell them out explicitly** in its response (with file locations and concrete guidance, per that agent’s instructions):
   - **Critical** issues (blocking defects),
   - **flaws** that matter for correctness, security, or maintainability (treat **Warnings** and material gaps as “flaws” in this sense),
   - **opportunities to optimize** the code (performance, clarity, structure, tests, or reviewability when the improvement is substantive, not mere nitpicking).
4. **Launch the developer subagent again** — the worker implements **all** optimizations, fixes, and follow-ups the reviewer documented in steps that block or materially improve quality (address **Critical** and **Warnings** in full; implement agreed **Suggestions** when they are clearly actionable in the same pass unless the user has narrowed scope).

## Loop discipline

- If the second developer pass produces a **large** or **risky** diff, the parent agent **may** run the code-quality-reviewer **once more** to confirm the follow-up did not introduce regressions; this is recommended when Critical items were fixed.
- If the reviewer approves with **only** non-blocking nits, **do not** relaunch the developer unless the user asked for polish on those nits.

## Parent agent responsibilities

- Pass a **single coherent brief** into the developer (scope, acceptance criteria, files to touch or avoid).
- After review, pass a **structured handoff** back to the developer: list of reviewer items to implement, in priority order, with no dropped Critical or Warning items.
- Keep **one concern per developer invocation** when possible so diffs stay reviewable.

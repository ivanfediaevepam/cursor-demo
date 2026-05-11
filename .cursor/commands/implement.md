# implement

You are running the **subagent development workflow**. Do **not** implement the user’s request inline in one shot unless they explicitly override this command.

## Required first step

Read and follow the full workflow in:

`.cursor/skills/subagent-dev-review-workflow/SKILL.md`

That skill is authoritative for agent selection, sequence, loop discipline, and handoff format.

## What you must do (summary)

1. **Developer subagent** — Spawn using configuration from `.cursor/agents/developer.md`. Give a single coherent brief: scope, acceptance criteria, files to touch or avoid, and any constraints from the user’s message.
2. **Code-quality-reviewer subagent** — After the developer finishes, spawn using `.cursor/agents/code-quality-reviewer.md` on the same change set.
3. **Developer again** — If the reviewer reports Critical issues, Warnings, or substantive Suggestions/optimizations, spawn the developer again with a structured handoff: reviewer items in priority order; do not drop Critical or Warning items.
4. **Optional second review** — If the follow-up diff is large or risky (especially after Critical fixes), you may run the reviewer once more per the skill’s loop discipline.

Use the same subagent mechanism this session supports (for example the Task tool with the appropriate subagent type, or project-specific `@` agent references). Pass **full task context** into the developer on every invocation.

When the workflow is complete, summarize outcomes for the user: what shipped, what the reviewer found, and what was fixed in the second pass (if any).

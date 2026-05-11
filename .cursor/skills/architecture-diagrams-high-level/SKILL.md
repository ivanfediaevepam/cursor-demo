---
name: architecture-diagrams-high-level
description: Creates and updates high-level architecture views (system context, containers, bounded contexts, trust boundaries) as Mermaid flowcharts or C4-style groupings. Use when the user asks for a high-level architecture diagram, context diagram, container view, system map, "birds-eye" or zoomed-out architecture, or to refresh an existing architecture diagram after code or ownership changes. For sequence, class, ER, state, or detailed component internals, use the create-diagrams skill instead.
disable-model-invocation: true
---

# High-level architecture diagrams

High-level means **one zoom level**: how major parts relate, not how a request walks through methods. Prefer **roughly 5–15 nodes**; if you need more, split into a context map plus per-context container views.

## Relationship to other skills

- **Mermaid syntax, shapes, styling, validation checklist** → follow [create-diagrams](../create-diagrams/SKILL.md).
- This skill only adds **scope rules** and **create vs update** workflow.

## What counts as high level

| In scope | Out of scope (use create-diagrams) |
| -------- | ---------------------------------- |
| Users, external SaaS, major apps/services | Individual classes, methods, props |
| Databases, queues, gateways as single nodes | Table-level ER (unless a tiny schema) |
| Bounded contexts / teams / deployable units | UI component trees |
| Sync vs async *between* containers | Step-by-step sequence across many messages |

Default diagram type: **`flowchart`** with **`subgraph`** for layers, trust boundaries, or bounded contexts. Reserve **sequence** for "one scenario, few participants" at container level only.

## Create workflow

1. **Audience and question** — e.g. onboarding ("what talks to what?") vs compliance ("trust boundaries"). State the assumed audience in one line if non-obvious.
2. **Inventory** — list external actors, deployables, datastores, async channels. Merge implementation detail into a single node (e.g. all React + BFF → `Web + BFF`).
3. **Draw** — declare direction (`LR` or `TD`). Label edges with **protocol or intent** (`HTTPS`, `AMQP`, `reads`, `publishes`). Apply `classDef` for 2–4 roles (external, internal, data, messaging).
4. **Place the artifact** — prefer the project’s doc home: root `README.md` under an **Architecture** section, or `docs/architecture.md` if that file exists or the user asks for a dedicated doc. Use a single fenced `mermaid` block per view.

## Update workflow

When the user asks to **update** an architecture diagram:

1. **Locate** — search the repo for ` ```mermaid ` near headings like Architecture, System context, or C4.
2. **Reconcile** — re-scan code/config (solution structure, `docker-compose`, ingress, package names, README) for new/removed services, renamed URLs, or new integrations.
3. **Edit minimally** — keep stable node IDs where possible so diffs stay readable; add/remove nodes and edges to match reality only.
4. **Call out uncertainty** — use `?` in labels or a short bullet list under the diagram for unverified boxes.
5. **Split if needed** — if the updated diagram exceeds ~20 nodes, follow the decomposition rules in create-diagrams (layer, bounded context, or C4 zoom).

## C4 alignment (without requiring the C4 plugin)

Think in C4 terms even when rendering plain Mermaid:

- **Context** — system as one box (or one subgraph), everything outside is actors/external systems.
- **Container** — runnable/deployable units (web app, API, worker, DB, broker).
- **Not here** — component/code zoom; route those to create-diagrams or separate lower-level diagrams.

## Quality bar

- Answers **one** question per diagram (see create-diagrams validation checklist).
- No invented systems; unknowns are marked or listed in prose.
- After a substantive update, ensure surrounding markdown still describes the same scope (adjust the intro sentence if the diagram’s purpose changed).

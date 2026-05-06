---
name: Git Code Review MCP
description: Review the current git diff (staged, unstaged, or a named range) and return a structured, actionable code review — inline comments anchored to file:line, a risk assessment, and a Conventional Commits message. Pulls the diff via a connected git MCP server when available, otherwise falls back to whatever git access is exposed (e.g., shell tools or pasted diff). Use whenever the user asks to review, audit, sanity-check, or "look over" their staged changes, current branch, last commit, PR, or a specific commit range; or asks for a commit message for current changes. Accepts a free-form scope (which changes to review, what to focus on) and respects it.
---

# Git Code Review MCP

A useful code review **catches what the author can't catch on their own**: contradictions between code and tests, weakened safety nets, drift from project conventions, and risk surfaces the author didn't realize they touched. It is _not_ a style-bikeshedding pass — when the project has a convention, the review enforces it; when it doesn't, the review doesn't invent one.

## Invocation pattern

Typically called with a free-form instruction, e.g.

> `/git-code-review-mcp Review my currently staged git changes for this Flight Status feature. Are there any missing edge cases, syntax issues, or architectural violations? Provide your feedback as inline comments and suggest a conventional commit message.`

Parse it into:

1. **Diff scope** — staged (`--staged` / `--cached`), unstaged, working tree, last commit (`HEAD~1..HEAD`), a named range, or a specific PR.
2. **Focus areas** — explicit asks ("missing edge cases, syntax, architectural violations") get prioritized; implicit areas (tests, security, dependencies) are still checked but flagged only when relevant.
3. **Output shape** — inline comments + risk + commit message (the default), or a subset if the user asked for less.

## Workflow

1. **Fetch the diff.** Try in this order:
   - **Connected git MCP** — look for tools matching `git_diff`, `git_status`, `git_log`, `git_show`. Use them. This is the preferred path.
   - **Shell access** — if a `bash`/`run` tool is connected and the working directory is a git repo, run `git diff --staged` (or whatever the scope dictates).
   - **Pasted diff** — if neither is available, ask the user to paste the diff or list changed files.

   Don't proceed to review without an actual diff; reviewing from memory or assumption produces wrong feedback.

2. **Categorize the changed files.** Frontend (`.tsx`/`.ts` under `src/frontend/`), backend (`.cs` under `src/backend/`), tests (`.spec.tsx`, `*Tests.cs`), CI (`.github/workflows/`), docs (`README.md`, `.md`), config (`*.json`, `*.csproj`, `package.json`).
3. **Review each file** against the relevant checklist below. For each finding, decide severity (Blocking / Concern / Nit) and write an inline comment anchored to `file:line`.
4. **Assemble the risk assessment** from the highest-severity findings plus the structural factors below.
5. **Draft the Conventional Commits message** based on the _primary_ purpose of the change.
6. **Verify** against the validation checklist before returning.

## Review checklist by category

### Correctness & logic

- Does each new branch actually return what it claims?
- Are there return paths the author missed (early returns, exceptions, async errors)?
- Off-by-ones, null/undefined deref, wrong equality (`==` vs `===`, reference vs structural), missing `await`.
- React: stale closures, missing dependency arrays, mutating state, unstable list `key`s.
- .NET: `IDisposable` not disposed, EF Core entities mutated outside tracked context, async without `await`.

### Tests

- Did any test get weakened? Watch for `.skip`, `[Fact(Skip = "…")]`, `assert.True(true)`, removed assertions, broadened matchers (`expect.any` replacing a specific value).
- Do new behaviors have tests? Public API changes without test deltas are a flag.
- Are negative paths covered (`DidNotReceive` / `not.toHaveBeenCalled`)?
- Tests that pass for any input ("smoke tests") are noted as not real coverage.

### Type safety

- `any`, `as any`, `@ts-ignore`, `@ts-expect-error` without an explanatory comment.
- C#: `dynamic`, `object`, suppression pragmas, `!` null-forgiving on lines where the null is real.
- Generic widening (`List<object>` instead of `List<T>`).

### Error handling

- Empty `catch` blocks. Logged-and-swallowed errors. Catch-all clauses where a specific exception would do.
- Missing validation on inputs that cross trust boundaries (HTTP body, query string, file content).
- `try/catch` added as a "fix" — flag and recommend addressing the cause.

### Dependencies

- New entries in `package.json` / `*.csproj` / `Directory.Packages.props`. Always flag — even if benign, the user usually wants to know.
- Removed entries (potential API breakage).
- Lockfile changes that don't match the manifest changes (suggests stale install).

### Security

- Secrets / API keys / tokens in code or config (especially `appsettings.json`, `.env`).
- SQL strings concatenated with user input.
- Disabled CSRF/auth checks. `[AllowAnonymous]` on actions that previously required auth.
- `dangerouslySetInnerHTML` without sanitization.
- New external HTTP calls without timeout/retry/error handling.
- `pull_request_target` in workflows; `permissions: write-all`; unpinned `@main` actions.

### Architecture & conventions

- New files in unexpected locations.
- New patterns that the rest of the project doesn't use (a `useReducer` in a project of `useState`s, a Mediator handler in a project of services, minimal APIs in a project of controllers).
- Public API changes (controller signatures, exported component props, type exports) — flag for backwards-compatibility check.
- Layer violations (controller calling repository directly when there's a service layer).
- Cross-cutting concerns reinvented (a new logging helper when the project already has one).

### Documentation

- Public methods / exported APIs added without XML / TSDoc.
- Stale docs left in place around changed code.
- README touched without setup/architecture changes (or vice versa).

### CI / build

- Workflow changes that broaden permissions or triggers.
- Pinning loosened (`@v4` → `@main`).
- Cache key changes that could miss.
- New required status checks not declared in branch protection (just note; the user owns config).

### Frontend specifics

- Accessibility regressions: `outline-none` without replacement focus ring, color-only signals, missing labels.
- Tailwind: class-merge order issues (`bg-red-100 bg-red-500`), arbitrary values overriding palette tokens.
- React keys using array index for reorderable lists.

### Backend specifics

- Async without cancellation tokens when siblings propagate them.
- Returning entities directly from controllers when DTOs are the convention.
- New exception types not registered in the project's exception filter (if there is one).

### Cleanup

- `console.log` / `Debug.WriteLine` / `Console.WriteLine`.
- Commented-out code.
- `// TODO` / `// FIXME` without an issue link.
- Unused imports left after edits.

## Inline comment format

Anchor every comment to `file:line`. Use a severity prefix and keep each comment to 1–3 sentences. Be **actionable** — suggest a fix, don't just complain.

```
src/frontend/src/components/FlightStatusBadge.tsx:23
[Concern] `Cancelled` uses `bg-red-500 text-red-300` (~2.4:1 contrast, fails WCAG AA).
Other statuses use the `bg-100/text-800` pattern; consider `bg-red-100 text-red-800`
for consistency and accessibility.
```

Severity prefixes (use plain text — no emoji unless the user's tooling expects them):

- **`[Blocking]`** — correctness bugs, security issues, missing tests for critical changes, weakened tests, secrets in code, breaking changes without migration. Must address before merge.
- **`[Concern]`** — smells, missed edge cases, conventions violated, type-safety holes, accessibility regressions. Should address or discuss.
- **`[Nit]`** — optional improvements, naming, redundant clauses. Author's discretion.
- **`[Question]`** — clarification needed; not actionable until answered.

Use `[Praise]` only when something genuinely deserves recognition (a clever non-obvious solution, a particularly thorough test). Don't use it as filler — it dilutes the signal.

## Risk assessment

Pick **Low / Medium / High** based on the highest-impact factor present, not by counting findings.

**High** — any of:

- A `[Blocking]` finding that hasn't been addressed.
- Public API / breaking change without explicit migration.
- Auth, payments, data integrity, or PII paths touched.
- Database migration included.
- New runtime dependency from an unfamiliar source.
- CI permissions widened or actions unpinned.

**Medium** — any of:

- Multiple `[Concern]` findings.
- Tests weakened or removed without replacement.
- New dependency (even if reputable).
- Cross-cutting refactor touching many files.
- Public API change with migration in place.

**Low** — none of the above; mostly localized changes with adequate tests.

State the assessment in one or two sentences explaining _which_ factor drove it: not "Medium because of multiple issues" but "Medium because the staged changes add a new dependency (`date-fns`) and weaken one existing test by replacing a specific assertion with `expect.anything()`".

## Conventional Commits message

Format:

```
<type>(<scope>): <description>

<body>

<footer>
```

- **Type:** `feat` (new behavior), `fix` (bug fix), `refactor` (no behavior change), `perf`, `test`, `docs`, `style`, `build`, `ci`, `chore`, `revert`.
- **Scope:** optional; area of the codebase (`flights`, `auth`, `frontend`, `ci`). Single word, lowercase.
- **Description:** imperative, lowercase, no trailing period, ~50 chars (`add status update endpoint`, not `Added the new status update endpoint.`).
- **Body:** optional; _why_, not _what_ (the diff already shows what). Wrap at ~72 chars.
- **Footer:** `BREAKING CHANGE: <description>` for breaking changes; `Closes #123` for issue refs.

If the staged changes legitimately span multiple types/scopes (e.g., a feat + an unrelated fix), suggest **splitting into separate commits** rather than a single mixed message — and provide one message per logical commit.

## Worked example — matching the invocation

> `Review my currently staged git changes for this Flight Status feature.`

Step 1 — fetch diff via the connected git MCP (or shell `git diff --staged` if no MCP). Suppose the diff covers:

- `src/backend/AviationApi/Controllers/FlightsController.cs` — new `UpdateStatus` action.
- `src/backend/AviationApi.Tests/Controllers/FlightsControllerTests.cs` — two new tests.
- `src/frontend/src/components/FlightStatusBadge.tsx` — new component.
- `src/frontend/src/components/FlightStatusBadge.spec.tsx` — three new tests.
- `src/frontend/src/services/Flight.ts` — added `FlightStatus` union and `UpdateFlightStatusRequest`.

A representative review output:

```
src/backend/AviationApi/Controllers/FlightsController.cs:48
[Concern] `UpdateStatusAsync` is called only on the success path, but the 404
path doesn't verify the service was *not* called. Consider adding a
`DidNotReceive` assertion in the corresponding test (already present — disregard).

src/frontend/src/components/FlightStatusBadge.tsx:14
[Concern] `Cancelled` uses `bg-red-500 text-red-300` (~2.4:1 contrast, fails AA).
Other statuses follow `bg-100/text-800`; align Cancelled with that pattern.

src/frontend/src/components/FlightStatusBadge.tsx:31
[Nit] `className ?? ''` — empty fallback is fine, but `${className ?? ''}`
inside a template literal allows a leading space when className is empty.
Consider trimming or conditionally including.

src/frontend/src/components/FlightStatusBadge.spec.tsx
[Question] Tests cover Landed and Delayed but not Cancelled — is that
intentional given this PR ships the badge as part of the feature?
```

**Risk: Medium.** The `Cancelled` contrast issue is user-visible and ships in the feature; no new dependencies, but tests don't yet cover the Cancelled case the user explicitly asked the badge to support.

**Suggested commit message:**

```
feat(flights): add status update endpoint and badge component

Introduce PUT /flights/{id}/status returning the updated flight, plus a
FlightStatusBadge component covering all FlightStatus values. Tests cover
the happy path and the not-found path on the backend, and two status
variants on the frontend.
```

If the diff also included an unrelated fix (say, a typo correction in `README.md`), the recommendation would be to unstage that file, commit the feature first, then commit the doc fix separately as `docs: fix typo in setup section`.

## Validation checklist (for the review itself)

Before returning the review, verify:

- [ ] An actual diff was fetched — not assumed from the user's description.
- [ ] Every inline comment anchors to a real `file:line` from the diff.
- [ ] Each comment has a severity prefix and is actionable (or marked `[Question]`).
- [ ] Findings reference project conventions where they exist; subjective preferences not sourced in conventions are downgraded to `[Nit]` or omitted.
- [ ] Risk level is justified by a specific factor, not by finding count.
- [ ] Commit message follows Conventional Commits; description is imperative and lowercase.
- [ ] If changes span multiple logical units, splitting was suggested.
- [ ] No file outside the diff is reviewed.
- [ ] The author wasn't praised for routine work; `[Praise]` is reserved for genuinely above-bar items.

## What to avoid

- Reviewing from memory or from the user's description without fetching the diff.
- Style nits when the project has no convention for them — that's the reviewer's preference, not the project's standard.
- Asking the author to make changes outside the diff scope ("while you're here, also fix…").
- Bikeshedding on naming when nothing is incorrect.
- "I would have written this differently" comments without a concrete improvement.
- Approving a Blocking finding because the rest of the diff looks good.
- Padding the review with `[Praise]` on routine work to soften the criticism.
- Risk assessments based on finding count rather than impact.
- Suggesting a commit message that mixes `feat` and `fix` when the changes should be split.
- Restating the diff in the summary — the diff is right there.
- Speculating about intent — use `[Question]` instead.
- Reviewing test files less rigorously than production code; weakened tests are a Blocking issue.

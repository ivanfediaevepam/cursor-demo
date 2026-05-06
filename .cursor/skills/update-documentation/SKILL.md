---
name: Update Documentation
description: Add or refresh inline comments, XML doc comments (.NET), TSDoc/JSDoc (TypeScript), and READMEs so the documentation matches the code's current behavior — focused on *why* and *how*, not on restating the signature. Use whenever the user asks to document, add comments to, "JSDoc this", "XML-comment this", "explain in code", or update a README; or mentions missing docs, stale docs, undocumented public API, or "the code is hard to read". Accepts a free-form scope (a method, a class, a file, "the recent changes") and respects it. Does NOT add OpenAPI/Swagger annotations or `<response>` tags — that is handled by the Create Swagger Doc skill, which is run separately.
---

# Update Documentation

Documentation should explain what the code can't say for itself: **intent, contract, and trade-offs**. A comment that paraphrases the next line is noise; a comment that captures _why_ the next line exists is leverage. The bar for adding docs is "would a future reader save time reading this?" — if not, leave it out.

## Invocation pattern

Typically called with a free-form instruction, e.g.

> `/update-documentation Our backend code is missing inline documentation. Please add standard XML summary comments to the new PUT method, explaining the parameters and what it returns.`

Parse it into:

1. **Scope** — a specific method, class, file, folder, or "the recent changes" (in which case look at unstaged/recently-modified files).
2. **Layer** — XML docs (.NET), TSDoc/JSDoc (TS), inline comments, README, or a mix.
3. **Depth** — `<summary>` only, full `<summary>`/`<param>`/`<returns>`, plus `<exception>`/`<remarks>`, or README-level architectural prose.
4. **Audience** — public API consumers, internal contributors, future-you debugging at 2am.

If the instruction names specific tags ("XML summary comments… parameters and returns"), produce exactly those — don't silently add `<remarks>`, `<example>`, or `<exception>` blocks the user didn't request.

## Workflow

1. **Identify the targets.** From the scope, list the specific symbols (methods, classes, types, exports) that need docs. For "the recent changes", scan modified files for public API surface that is undocumented or whose docs predate the change.
2. **Inspect the code.** Read each target's body. Note: parameters, return shape, thrown exceptions, side effects, business rules, non-obvious algorithm steps, performance characteristics, and any "this is weird because…" decisions worth capturing.
3. **Inspect a documented sibling.** Open one or two already-documented files in the same project to match: tag set, sentence style (imperative vs descriptive), level of detail, period usage, line-wrap conventions.
4. **Write the doc**, layer-appropriate (XML / TSDoc / inline / README).
5. **Verify** against the validation checklist — including the "is this just restating the signature?" check.

## Documentation philosophy

- **Why over what.** The signature already says _what_; docs add intent, constraints, and edge cases.
- **Contract over implementation.** Public-API docs describe behavior callers can rely on, not how the method achieves it. Implementation details belong in inline comments at the relevant line, not in the public summary.
- **Don't lie.** A wrong doc is worse than no doc — it actively misleads readers and tooling. When updating code, update the surrounding docs in the same change.
- **Don't list the obvious.** A `<param name="id">The id.</param>` is filler. Say something a careful reader couldn't infer (`The flight identifier; must be a positive integer.`).
- **No metadata in code.** Skip `@author`, `@date`, `@version` — git handles those.

## .NET XML documentation

The standard tag set, in priority order:

| Tag                    | When to add                                                                                       |
| ---------------------- | ------------------------------------------------------------------------------------------------- |
| `<summary>`            | Every public type, method, property, event. One sentence describing intent.                       |
| `<param name="x">`     | Every parameter, when its meaning isn't obvious from the name and type.                           |
| `<returns>`            | Every method that returns a value. Describe what callers can rely on, including null/empty cases. |
| `<exception cref="T">` | Each documented exception the method throws, with the condition.                                  |
| `<remarks>`            | Extra context: side effects, threading, performance. Sparingly.                                   |
| `<paramref name="x"/>` | When referring to a parameter in prose.                                                           |
| `<see cref="X"/>`      | Cross-references to related types/members.                                                        |
| `<inheritdoc/>`        | Overrides and interface implementations whose contract is unchanged.                              |
| `<example>`            | Reserved for SDK / library APIs, not internal code.                                               |

Style:

- Start `<summary>` with a verb in third-person present (`Updates`, `Returns`, `Validates`) — not "This method updates…".
- One sentence; end with a period.
- Wrap parameter and return descriptions to ~100 chars per line if siblings do.

```csharp
/// <summary>Updates the status of an existing flight.</summary>
/// <param name="id">The identifier of the flight to update; must be positive.</param>
/// <param name="request">The new status value.</param>
/// <returns>
/// 200 OK with the updated flight; 404 Not Found when no flight matches <paramref name="id"/>.
/// </returns>
```

**Out of scope here:** `<response code="...">` tags and the matching `[ProducesResponseType]` / `[SwaggerOperation]` attributes — the Create Swagger Doc skill handles those. The two skills coexist on the same method: this one writes the prose contract, the Swagger skill writes the API-surface annotations.

## TypeScript TSDoc / JSDoc

In a TypeScript codebase, types are in the signature — TSDoc focuses on **semantics**, not type repetition.

| Tag           | When to add                                                                                                          |
| ------------- | -------------------------------------------------------------------------------------------------------------------- |
| Description   | Every exported function, class, type. One sentence opening.                                                          |
| `@param x`    | When the meaning isn't obvious from the name + type. **No `{type}`** — TypeScript already has it.                    |
| `@returns`    | When the return semantics aren't obvious from the type (e.g., "null when not found", "empty array when no matches"). |
| `@throws`     | Documented errors callers should handle.                                                                             |
| `@example`    | For reusable utilities and public package entry points.                                                              |
| `@see`        | Cross-references.                                                                                                    |
| `@deprecated` | With migration instructions and a target removal version if known.                                                   |
| `@remarks`    | Side effects, performance, edge cases.                                                                               |

```ts
/**
 * Returns the Tailwind palette classes for a given flight status.
 *
 * @param status - The status whose palette to look up.
 * @returns A space-separated class string suitable for `className`.
 *
 * @remarks
 * The mapping is exhaustive over `FlightStatus`; adding a new member to the
 * union without updating this map will fail type-checking at build time.
 */
export function getStatusPalette(status: FlightStatus): string {
  /* … */
}
```

Style:

- Hyphenated `@param x - description` is the TSDoc convention; `@param x description` (no hyphen) is JSDoc. Match siblings.
- Don't write `@param {string} status` in TS files — the type is already in the signature; restating it duplicates and rots.
- For React components, document the component itself; types on the props interface get their own per-property comments where the meaning isn't obvious.

```ts
interface FlightStatusBadgeProps {
  /** The flight status the badge represents. */
  status: FlightStatus;
  /** Extra Tailwind classes appended to the badge. */
  className?: string;
}
```

## Inline comments

Reserve inline comments for one of:

- **Why**: a non-obvious decision (`// retry once — Stripe webhooks occasionally arrive twice`).
- **Workaround**: a hack with a pointer (`// TODO(#1234): remove once upstream fixes …`).
- **Business rule** that doesn't show up in the code (`// US flights skip customs; international flights branch below`).
- **Performance note**: when the obvious code would be slower (`// avoid LINQ here — hot path, allocates per call`).

Avoid:

- Comments that paraphrase the next line (`// loop over flights`).
- Block comments at the top of every method (`<summary>` exists for a reason in .NET; TSDoc in TS).
- Commented-out code — delete it; git remembers.
- `// HACK` / `// FIXME` without context — explain or open an issue.

## README updates

The two READMEs (`src/backend/README.md`, `src/frontend/README.md`) cover **setup, architecture, and conventions** — not the per-file or per-method documentation that lives in code.

Update the README when one of the following changes:

- New runtime / SDK / Node version requirement.
- New environment variable, config file, or secret.
- New top-level command (`npm run …`, `dotnet …`).
- New high-level architectural component (a new service, a new bounded context, a new third-party integration).
- A change to "how to add a new feature" — the path users follow.

For these, **propose the change** to the user (show the diff and explain) before writing it; READMEs are read by every new contributor and changes deserve review.

For smaller updates (a typo, a stale dependency version, a renamed script), apply directly.

Don't put in a README:

- Per-method documentation that belongs in code.
- Auto-generatable content (file trees, every endpoint).
- Roadmaps and aspirations — those belong in issues.

## Worked example — matching the invocation

> `Add standard XML summary comments to the new PUT method, explaining the parameters and what it returns.`

Assumption stated inline: the method is `FlightsController.UpdateStatus(int id, UpdateFlightStatusRequest request)` returning `ActionResult<FlightDto>`.

```csharp
/// <summary>Updates the status of an existing flight.</summary>
/// <param name="id">The identifier of the flight to update; must be positive.</param>
/// <param name="request">The status change to apply.</param>
/// <returns>
/// An HTTP 200 response carrying the updated <see cref="FlightDto"/> when the flight exists,
/// or HTTP 404 when no flight matches <paramref name="id"/>.
/// </returns>
[HttpPut("{id:int}/status")]
public async Task<ActionResult<FlightDto>> UpdateStatus(
    [FromRoute] int id,
    [FromBody]  UpdateFlightStatusRequest request)
{
    // … unchanged …
}
```

Notes on what this does and doesn't do:

- **Three tags**, exactly the ones requested: `<summary>`, `<param>` ×2, `<returns>`.
- **`<returns>` describes both branches** — success and the 404 — because that's what callers need to handle.
- **`<paramref>` and `<see cref>`** for cross-referencing — these light up tooltips in IDEs and aren't free filler.
- **No `<exception>`, `<remarks>`, or `<response code>` blocks added** — the user didn't ask for them. The `<response>` annotations live in the Swagger skill's scope anyway.
- **Method body is unchanged.** Documentation is purely additive.

## Validation checklist

Before finishing, verify:

- [ ] Every targeted symbol got the tags the user asked for — and only those.
- [ ] Each `<summary>` / TSDoc opening is one sentence, third-person, ending with a period.
- [ ] No `<param>` or `@param` is just `<param name="x">x</param>` — it adds intent or constraint, or it's omitted.
- [ ] Return descriptions cover edge cases (null, empty, exceptions, both HTTP branches for actions).
- [ ] No type information duplicated in TSDoc `@param` — types stay in the signature.
- [ ] No `@author`, `@date`, `@version`, or other git-tracked metadata added.
- [ ] No commented-out code introduced.
- [ ] Existing docs left untouched outside the requested scope (no opportunistic reformatting).
- [ ] README changes affecting setup/architecture were proposed (diff + rationale), not silently applied.
- [ ] Code behavior unchanged — this skill never edits logic.
- [ ] No Swagger/OpenAPI annotations added (separate skill).

## What to avoid

- Restating the signature: `<summary>Gets the user.</summary>` on `GetUser()` — say nothing or say something useful.
- Documenting trivial getters/setters and obvious property names.
- Adding `@param {string}` in TypeScript JSDoc — the compiler already has the type.
- Writing `<remarks>` blocks longer than the method body.
- Adding `<example>` to every internal method — examples belong on library/SDK surface and shared utilities.
- Inline comments that paraphrase the next line.
- Block-comment banners (`/****** SECTION: HELPERS ******/`) — file structure should make sections obvious.
- Documenting private helpers as if they were public API.
- Reformatting unrelated existing docs while passing through.
- "TODO" and "FIXME" without an issue link or actionable context.
- Editing the code to make it easier to document — flag the design issue instead.
- Touching API-contract annotations (`<response>`, `[ProducesResponseType]`, `[SwaggerOperation]`) — those belong to the Create Swagger Doc skill.

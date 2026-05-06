---
name: Bug Fix
description: Diagnose, minimally fix, and regression-test bugs in the codebase — find the root cause, change as little as possible, and prove the fix with a test that fails without it. Use whenever the user reports something is broken, wrong, off, regressed, leaking, returning the wrong value, displaying incorrectly, or behaving differently than expected; or asks to "fix", "debug", "investigate", "track down", or "patch" an issue. Accepts a free-form bug report (symptom + suspected location) and respects scope — doesn't refactor or "improve" code while passing through. Generates a regression test when no test currently covers the bug.
---

# Bug Fix

A good bug fix has four properties: it addresses the **root cause**, it **changes only what's needed**, it ships with a **test that proves it**, and it **explains the why** when the cause isn't obvious from the diff. A fix without those properties is gambling — it might work today and silently re-break next month.

## Invocation pattern

Typically called with a free-form bug report, e.g.

> `/bug-fix There is a bug: when the status is "Cancelled", the text color in the badge is hard to read against the red background. Find the issue in the Tailwind classes, fix it safely to ensure good contrast, and update the related test if necessary.`

Parse it into:

1. **Symptom** — the user-visible behavior ("text is hard to read on red").
2. **Suspected location** — the user's pointer ("Tailwind classes" → `FlightStatusBadge.tsx`). Treat this as a hint, not a verdict.
3. **Conditions** — when it happens ("status is Cancelled").
4. **Expected behavior** — what should happen instead ("good contrast"), explicit or inferred.
5. **Test scope** — "update the related test", "add a regression test", or silent (default to adding one when no test covers the path).

## Workflow

1. **Reproduce or fully understand the failure.** Open the file(s) the user pointed to. Read the relevant code path top to bottom. If reproduction requires running the app and that's not possible, walk the code mentally and identify the exact line(s) where the wrong behavior originates.
2. **Find the root cause, not the symptom.** Ask "why does this line produce that output?" until the answer is "because of this specific bug" — a wrong constant, a missing branch, an off-by-one, a wrong operator, a stale closure, a missing await, a contrast-failing class pair. Stop only when fixing one thing fixes the symptom, and you can articulate why.
3. **State the cause and the proposed fix in one sentence** before writing code. ("`Cancelled` uses `bg-red-500` with `text-red-300`, which fails AA contrast; switch to `bg-red-100 text-red-800` to match the other statuses' light-bg pattern.") If the sentence is fuzzy, the diagnosis isn't done.
4. **Make the smallest change that fixes the cause.** No renames, no extractions, no reformatting, no "while I'm here" cleanup. If the surrounding code has problems, list them as follow-ups — don't fold them in.
5. **Add a regression test.** It must fail without the fix and pass with it. Place it next to the SUT, using the project's existing test conventions.
6. **Verify.** Re-read the changed lines and the test as a pair. The test should describe the bug ("Cancelled badge has high-contrast text"), not the fix.

## Root cause discipline

- **Symptom ≠ cause.** "The badge looks bad" is a symptom; "`text-red-300` on `bg-red-500` is 2.4:1 contrast" is a cause. Patches that target the symptom (a CSS override at a parent level) leave the cause in place to bite again.
- **One cause, one fix.** If you find two unrelated bugs in the same file, file the second separately and fix one at a time. Mixing them makes the regression test ambiguous and the diff hard to review.
- **Beware the "fixed it but don't know why" trap.** If the fix worked but you can't explain it, the cause is still there — you've just shifted the timing or location. Keep digging until you can articulate it.
- **Trust the user's symptom, question the user's diagnosis.** Reports of _what_ broke are usually accurate. Reports of _where_ it broke are sometimes off by a layer — verify before patching.

## Common bug categories

When the cause isn't obvious, these are the categories that turn up most often in this codebase:

- **Off-by-one** — loop bounds, array indices, date ranges, pagination cutoffs, ID-vs-Index confusion.
- **Null / undefined dereference** — missing `?.`, missing default value, missing `if (x is null)` guard.
- **Wrong equality** — `==` vs `===` in TS, reference vs structural equality in C# (`Equals` vs `==`), case-sensitive string compare where it shouldn't be.
- **Async ordering** — missing `await`, fire-and-forget `Task`, stale closure capturing old state in React, race between two awaits.
- **State mutation** — mutating React state directly, mutating EF Core entities outside a tracked context, sharing a list reference across requests.
- **Stale cache / memoization** — `useMemo` with a missing dep, HTTP response cached past TTL, EF Core query cache.
- **Missing endpoint or wrong route** — frontend calls a path the backend doesn't expose, route param type mismatch (`{id}` vs `{id:int}`), trailing-slash handling.
- **Wrong key in React lists** — `key={index}` causing reconciliation glitches when items reorder.
- **CSS / Tailwind specificity** — class merge order (`bg-red-100 bg-red-500` — last wins), arbitrary-value classes overriding palette tokens, dark-mode variant missing.
- **Accessibility regressions** — color contrast (this skill's worked example), missing labels, focus traps, keyboard navigation gaps.
- **Time zone / date** — `DateTime` vs `DateTimeOffset`, UTC vs local, day-boundary off-by-ones from naive comparisons.
- **Floating-point comparison** — `==` on doubles, currency stored as `double` instead of `decimal`.
- **Encoding** — URL encoding, HTML escaping, JSON serialization defaults (camelCase vs PascalCase).
- **Boundary conditions** — empty list, single item, very large list, concurrent users, exact threshold values.

## Fix scope rules

- **Touch only the lines that fix the bug**, plus the regression test, plus a one-line comment if the fix is non-obvious.
- **Don't rename**, even when names are bad. Renames change the diff's reviewability.
- **Don't extract methods or components**, even when the function is too long.
- **Don't reformat or re-order imports**, even when they're inconsistent.
- **Don't update unrelated dependencies**, configs, or comments.
- **Don't tighten or loosen types** beyond what the fix requires.
- **Don't add try/catch as a fix.** Catching an exception is hiding a bug, not fixing one. The exception means something — find why it was thrown and address that.

If the surrounding code has problems worth addressing, list them in the response as **suggested follow-ups** with one-line rationale each. Let the user decide whether to widen scope.

## Regression test rules

A regression test is the proof the bug existed and is gone. Without it, the next refactor can quietly resurrect the same bug.

- **Fails without the fix, passes with it.** This is the test's job. If you can't explain how the test would fail on the original code, the test isn't proving anything.
- **Tests the bug, not the fix.** Assert the user-observable behavior ("Cancelled badge has high-contrast text classes"), not the implementation ("badge has `text-red-800`" — close, but the test should still pass if someone later switches to `text-white` on `bg-red-600`, which also has high contrast).
- **Lives next to the existing tests** for the same SUT, in the same style. Use Test Generation .NET / JS skills' conventions.
- **Names the bug.** `Cancelled_BadgeUsesAccessibleContrast` or `cancelled_status_renders_with_readable_contrast` — readable as a behavior statement.
- **One regression test per fix.** If a single fix actually addresses multiple bugs, that's a sign you're fixing more than one thing.

When no regression test is feasible (e.g., bug is in a script that can't be unit-tested in the current setup), say so explicitly and write a `// REGRESSION:` comment at the fix site explaining what to verify manually.

## When to widen scope or escalate

Stop and check with the user before continuing if you find:

- **The fix requires changing a public API** (controller signature, exported function shape).
- **Multiple call sites** would need to be updated.
- **The fix changes behavior visible to users beyond what was reported** (e.g., fixing this also changes how `Delayed` renders).
- **A data migration** is needed.
- **The "bug" is actually a missing feature**, not a defect.
- **You can't reproduce or pin down the cause** after a focused look — guessing patches is worse than asking.

## Worked example — matching the invocation

> `When the status is "Cancelled", the text color in the badge is hard to read against the red background. Find the issue in the Tailwind classes, fix it safely to ensure good contrast, and update the related test if necessary.`

**Step 1 — Inspect.** Open `src/frontend/src/components/FlightStatusBadge.tsx`. Read the `statusStyles` map.

**Step 2 — Diagnose.** (Hypothetical reading; would be confirmed against the actual file.) The `Cancelled` row maps to `bg-red-500 text-red-300 ring-red-400`. That's `text-red-300` on `bg-red-500` — roughly 2.4:1 contrast, well below WCAG AA's 4.5:1 for normal text. Every other status uses the `bg-{color}-100 text-{color}-700/800` pattern, which sits comfortably above AA. The `Cancelled` entry deviates from that pattern.

**Step 3 — Cause sentence.** _`Cancelled` was authored with a saturated background and a desaturated foreground, breaking the project's `bg-100/text-800` contrast pattern; align it with the other statuses._

**Step 4 — Minimal fix.**

```diff
 const statusStyles: Record<FlightStatus, string> = {
   Scheduled: 'bg-slate-100 text-slate-700 ring-slate-200',
   Boarding:  'bg-blue-100 text-blue-700 ring-blue-200',
   Delayed:   'bg-yellow-100 text-yellow-800 ring-yellow-200',
   InAir:     'bg-indigo-100 text-indigo-700 ring-indigo-200',
   Landed:    'bg-green-100 text-green-700 ring-green-200',
-  Cancelled: 'bg-red-500 text-red-300 ring-red-400',
+  Cancelled: 'bg-red-100 text-red-800 ring-red-200',
 };
```

One row changed. No reformat, no rename, no other adjustments.

**Step 5 — Regression test.** In `FlightStatusBadge.spec.tsx`:

```tsx
it("renders Cancelled with accessible text/background contrast", () => {
  render(<FlightStatusBadge status="Cancelled" />);
  const badge = screen.getByText("Cancelled");
  // The bug shipped `bg-red-500 text-red-300` (~2.4:1, fails AA).
  // The fix aligns Cancelled with the project's bg-100/text-800 pattern.
  expect(badge).toHaveClass("bg-red-100", "text-red-800");
});
```

If a `Cancelled` test already exists asserting the old classes, **update it** rather than adding a duplicate. The comment explains the bug to whoever reads the test next so they don't "simplify" the assertion away.

**Step 6 — Verify.** Re-read the diff and the test as a pair. The test fails on the pre-fix file (assertion mismatches `bg-red-500`/`text-red-300`) and passes on the post-fix file. The change is one row plus one test.

**Optional follow-up to flag, not fold in:** if the project has other places that use `bg-{color}-500 text-{color}-300` patterns (filter buttons, error banners), those may have the same contrast issue. Mention this as a suggested separate ticket — don't fix it in this change.

## Validation checklist

Before finishing, verify:

- [ ] The cause is articulated in one sentence — not "tweaked the styles" but "swapped the saturated bg/desaturated fg for the project's bg-100/text-800 pattern".
- [ ] The diff touches only what the cause requires (plus test, plus an explanatory comment if non-obvious).
- [ ] No rename, no extraction, no reformat, no unrelated edits.
- [ ] No `try/catch` added as a "fix".
- [ ] Regression test exists, fails without the fix, passes with it, and asserts behavior — not implementation trivia.
- [ ] If an existing test covered the wrong behavior, it was updated (not duplicated).
- [ ] Public API surface unchanged unless the user authorized that.
- [ ] Behavior outside the reported scenario is unchanged (or changes are flagged).
- [ ] Surrounding-code issues are listed as follow-ups, not folded into this fix.

## What to avoid

- "Fixed it but I'm not sure why." If you can't explain the cause, the cause is still in the code.
- Fixing the symptom one layer up (CSS override on the parent, exception swallow in a wrapper).
- Rewriting the function while fixing one line in it.
- Adding broad `null` guards everywhere instead of finding why a null is reaching the line.
- Changing a public signature to make a private bug easier to fix.
- Skipping the regression test because "the fix is obvious".
- Asserting implementation details in the regression test (specific call counts, internal helper invocations) — assert the behavior, not the path.
- Folding in unrelated bug fixes you noticed along the way.
- Bumping versions, reformatting, or re-sorting imports as part of the fix.
- Marking something `// TODO` instead of fixing it or filing it.
- Disabling a failing test to make CI green — failing tests are signal, not noise.
- "Defensive" `catch` blocks that turn a known bug into a silent one.

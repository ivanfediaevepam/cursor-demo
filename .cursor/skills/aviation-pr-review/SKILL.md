---
name: Aviation PR Review
description: Runs the team's PR checklist on all changed files. Flags new dependencies, silent error swallowing, weakened test assertions, and 'any' types. Runs lint and tests, then outputs a risk-graded summary and suggested commit message.
---

# Aviation PR Review

When triggered, you MUST perform a comprehensive code review of all files changed in the current workspace or provided diff.

## Review Checklist
Read every changed file and flag the following:
1. **New Dependencies**: Flag any new libraries or packages added to package.json or C# project files.
2. **Silent Error Swallowing**: Flag empty catch blocks (`catch { }` or `catch (Exception) { }`) with no logging or rethrowing.
3. **Weakened Test Assertions**: Flag test assertions that were changed to be less strict (e.g., `Assert.True` instead of `Assert.Equal`, or `expect(x).toBeTruthy()` instead of `expect(x).toBe(...)`).
4. **Types**: Flag the usage of `any` in TypeScript files.
5. **Hardcoded Values**: Flag any hardcoded configuration values, magic numbers, or overridden seed data.

## Actions to perform
1. Read the diff/changes and apply the checklist above.
2. Run linters and tests (if commands are available/known) or ask the user to confirm test status.
3. Identify the risk level of the changes:
   - **Low Risk**: Documentation, minor style tweaks, safe renames.
   - **Medium Risk**: Logic changes, bug fixes, adding new small components.
   - **High Risk**: Adding new dependencies, weakening tests, swallowing errors, or modifying core architecture.

## Output Format
Your final output MUST follow this structure exactly:

### 🚦 Risk Level: [Low Risk / Medium Risk / High Risk]
[1-2 sentence explanation of why this risk level was chosen based on the findings]

### 🔍 Findings
Group your findings by file. If a file has no findings, you don't need to list it.
- **`[filename]`**
  - [Finding 1]
  - [Finding 2]

### 📝 Suggested Commit Message
Provide a suggested commit message following conventional commits format. For example:
`feat: add RangeInKm to Plane domain`
`fix: correct GetFlightById index mapping`

---
name: Generate GitHub Action
description: Author GitHub Actions workflow YAML for this repository's CI/CD needs — covering .NET (backend) and React/Node (frontend) jobs, triggers, caching, permissions, concurrency, path filters, and artifact handling. Use whenever the user asks to add, create, write, or update a GitHub Action, workflow, pipeline, CI, CD, or automation YAML; or mentions `.github/workflows`, `actions/setup-*`, `dotnet build`, `npm ci`, "build matrix", "release workflow", "cron job", or "deploy on tag" in a CI context. Accepts a free-form instruction describing triggers, jobs, runtime versions, and artifacts, and respects it.
---

# Generate GitHub Action

Produce workflow YAML that is **secure by default, fast on warm caches, and faithful to the invocation**. A workflow that does more than was asked is as wrong as one that does less — extra triggers run on the wrong events, extra jobs slow PRs, extra permissions create attack surface.

## Invocation pattern

Typically called with a free-form instruction, e.g.

> `/generate-github-action We need a CI pipeline for our new full-stack feature. Create a GitHub Actions workflow that triggers on pull requests to main. Run two parallel jobs: build/test the .NET backend, and npm install/build for the React frontend.`

Parse the instruction into:

1. **Workflow purpose** — CI / release / deploy / scheduled / manual.
2. **Triggers** — events + branches/tags/paths.
3. **Jobs** — what runs, in what order, what they depend on.
4. **Runtimes** — .NET version, Node version (read from project files when possible — see below).
5. **Outputs** — artifacts, test reports, coverage, deployment targets.

If the instruction names triggers explicitly (e.g. "on pull requests to main"), use exactly those — do not silently add `push` or other branches. If a field is genuinely ambiguous, ask once; otherwise pick the convention below and state the assumption.

## Workflow steps

1. **Detect runtime versions from the repo, not from memory.**
   - .NET: Use `dotnet-version: '10.x'` as the project is .NET 10.
   - Node: Use `node-version: '20'` as the project uses Node 20 LTS.
2. **Locate project paths.** Backend at `src/backend/AviationApi/` (solution at `src/backend/`); frontend at `src/frontend/` (where `package.json` lives). Use these as `working-directory` and as path filters.
3. **Decide file layout.** One workflow with parallel jobs (default for "CI for the whole repo") vs. split files (`backend-ci.yml`, `frontend-ci.yml`) when the cadences or owners differ. Match what's already in `.github/workflows/`.
4. **Compose the workflow** using the structure rules below.
5. **Verify** against the validation checklist.

## File placement and naming

- All workflows live in `.github/workflows/`.
- Names are kebab-case `.yml` (`ci.yml`, `backend-ci.yml`, `release.yml`, `nightly.yml`).
- The `name:` field at the top should describe what the workflow _does_, not its filename ("Backend CI", not "backend-ci").
- Job IDs are kebab-case (`build-backend`); job `name:` fields are human-readable ("Build backend").

## Workflow structure — required components

Every workflow Claude generates includes these top-level keys, in this order:

```yaml
name: <human-readable workflow name>

on:
  # exactly the triggers the user asked for; no extras
  pull_request:
    branches: [main]
    paths:
      - "src/backend/**"
      - "src/frontend/**"
      - ".github/workflows/<this-file>.yml"

permissions:
  contents: read # least privilege; widen only when a step needs it

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

jobs:
  # ...
```

Rules:

- **Pin action versions** to a major tag (`actions/checkout@v4`) for first-party actions; pin third-party actions to a full commit SHA when the supply chain matters. Never `@main` or `@latest`.
- **Permissions are read-only by default.** Add `pull-requests: write`, `id-token: write`, `packages: write`, etc. only on jobs that genuinely need them — and only at the job level, not the workflow level.
- **Concurrency cancels superseded runs** on the same ref so a fresh push doesn't queue behind stale ones.
- **Path filters** prevent the workflow from running on unrelated changes (docs-only PRs, etc.). Always include the workflow file itself in the filter so changes to it are tested.
- **Pin the runner.** `runs-on: ubuntu-24.04` is more reproducible than `ubuntu-latest` for CI you intend to keep stable. `ubuntu-latest` is fine if the project's existing workflows use it.

## .NET job pattern

```yaml
build-backend:
  name: Build & test backend
  runs-on: ubuntu-24.04
  defaults:
    run:
      working-directory: src/backend
  steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: "10.x"
        cache: true
        cache-dependency-path: src/backend/**/packages.lock.json

    - name: Restore
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Test
      run: >
        dotnet test
        --configuration Release
        --no-build
        --logger "trx;LogFileName=test-results.trx"
        --results-directory ./TestResults

    - name: Upload test results
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: backend-test-results
        path: src/backend/TestResults/*.trx
```

Notes:

- `cache: true` on `setup-dotnet@v4` requires `packages.lock.json` files; if the repo doesn't use lockfiles, omit the cache or enable `RestorePackagesWithLockFile` first.
- `--no-restore` on build and `--no-build` on test save time and surface mismatches early.
- `if: always()` on the upload step ensures test results upload even when tests fail (the common case where you most want them).

## React/Node job pattern

```yaml
build-frontend:
  name: Build frontend
  runs-on: ubuntu-24.04
  defaults:
    run:
      working-directory: src/frontend
  steps:
    - uses: actions/checkout@v4

    - name: Setup Node
      uses: actions/setup-node@v4
      with:
        node-version: "20"
        cache: "npm"
        cache-dependency-path: src/frontend/package-lock.json

    - name: Install
      run: npm ci

    - name: Lint
      run: npm run lint --if-present

    - name: Test
      run: npm test --if-present -- --ci

    - name: Build
      run: npm run build

    - name: Upload build artifact
      uses: actions/upload-artifact@v4
      with:
        name: frontend-dist
        path: src/frontend/dist
```

Notes:

- `npm ci` is mandatory for CI (deterministic, requires lockfile). Never `npm install` in CI.
- `--if-present` lets the workflow tolerate missing scripts without failing — useful when not every package defines `lint`/`test`.
- Cache key derives from the lockfile path; pointing it at the right file is what makes the cache effective.

## Worked example — matching the invocation

> `Triggers on PRs to main. Two parallel jobs: .NET backend build/test, React frontend npm install/build.`

```yaml
name: CI

on:
  pull_request:
    branches: [main]
    paths:
      - "src/backend/**"
      - "src/frontend/**"
      - ".github/workflows/ci.yml"

permissions:
  contents: read

concurrency:
  group: ci-${{ github.ref }}
  cancel-in-progress: true

jobs:
  build-backend:
    name: Build & test backend
    runs-on: ubuntu-24.04
    defaults:
      run:
        working-directory: src/backend
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: "10.x"
          cache: true
          cache-dependency-path: src/backend/**/packages.lock.json
      - run: dotnet restore
      - run: dotnet build --configuration Release --no-restore
      - run: dotnet test --configuration Release --no-build --logger "trx;LogFileName=test-results.trx"
      - if: always()
        uses: actions/upload-artifact@v4
        with:
          name: backend-test-results
          path: src/backend/**/TestResults/*.trx

  build-frontend:
    name: Build frontend
    runs-on: ubuntu-24.04
    defaults:
      run:
        working-directory: src/frontend
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: "20"
          cache: "npm"
          cache-dependency-path: src/frontend/package-lock.json
      - run: npm ci
      - run: npm run build
      - uses: actions/upload-artifact@v4
        with:
          name: frontend-dist
          path: src/frontend/dist
```

Two jobs, no `needs:` between them — they run in parallel because that's what was asked. Triggers are exactly `pull_request` to `main`; `push` was deliberately not added.

## Trigger reference

| Need                                | Trigger block                                            |
| ----------------------------------- | -------------------------------------------------------- |
| CI on PRs only                      | `on: pull_request: { branches: [main] }`                 |
| CI on PRs + protected-branch pushes | `on: { pull_request: ..., push: { branches: [main] } }`  |
| Release on tag                      | `on: push: { tags: ['v*.*.*'] }`                         |
| Manual run                          | `on: workflow_dispatch:` (with `inputs:` for parameters) |
| Scheduled                           | `on: schedule: [{ cron: '0 6 * * *' }]` (UTC)            |
| Reusable                            | `on: workflow_call:`                                     |

`pull_request_target` is **not** a default — it runs in the base repo's context with secrets and is easy to misuse. Only use it when you've explicitly considered the security implications.

## Optimization patterns

- **Cache aggressively, invalidate correctly.** `setup-dotnet@v4` and `setup-node@v4` handle this when given the right `cache-dependency-path`.
- **Path filters** keep PRs that touch only docs from running CI.
- **Matrix** when you actually test on multiple platforms/versions — not as decoration. `strategy: { matrix: { os: [ubuntu-24.04, windows-latest] } }`.
- **`needs:`** chains jobs only when there's a real dependency (deploy after build). For independent work, parallel is faster.
- **Composite actions / reusable workflows** (`uses: ./.github/actions/...` or `workflow_call`) when the same setup repeats across files.

## Validation checklist

Before finishing, verify:

- [ ] Workflow file is in `.github/workflows/` with a kebab-case `.yml` name.
- [ ] `name:` is human-readable; job IDs are kebab-case.
- [ ] Triggers match the invocation exactly — no silently added events.
- [ ] All actions are pinned to a tag or SHA; no `@main` or `@latest`.
- [ ] `permissions:` is present and as narrow as possible.
- [ ] `concurrency:` cancels superseded runs on the same ref.
- [ ] Path filters include the workflow file itself.
- [ ] Runtime versions are hardcoded (`dotnet-version: '10.x'`, `node-version: '20'`) since the project lacks explicit version files.
- [ ] Caching is configured with the correct `cache-dependency-path`.
- [ ] `working-directory` is set instead of `cd` in `run` steps.
- [ ] CI uses `npm ci` (never `npm install`) and `dotnet restore` before build.
- [ ] Test result artifacts upload with `if: always()`.
- [ ] Secrets are referenced as `${{ secrets.NAME }}` and never echoed.
- [ ] No new GitHub Apps / marketplace actions introduced beyond what the project already trusts (or the user explicitly asked for).

## What to avoid

- Pinning to `@main`, `@master`, or `@latest`.
- `permissions: write-all` or omitting `permissions:` (defaults to whatever the repo grants).
- `pull_request_target` without an explicit reason in a comment.
- Echoing secrets, even masked: `run: echo ${{ secrets.TOKEN }}`.
- `npm install` in CI.
- `continue-on-error: true` on the actual build/test steps (only acceptable on auxiliary steps like coverage upload).
- Adding deploy / publish / release steps when the user asked only for CI.
- Adding `push` triggers when the user asked only for `pull_request`.
- One mega-workflow that handles CI, release, and deploy in branching `if:` conditions — split by purpose.
- Inline shell scripts longer than ~10 lines — extract to `scripts/` and invoke.

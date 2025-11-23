# Tasks: Interactive C# 14 Code Execution PoC (Backend-first)

Feature Dir: /Users/antbly/dev/netcoder/specs/001-build-a-proof

Plan: /Users/antbly/dev/netcoder/specs/001-build-a-proof/plan.md

Contracts: /Users/antbly/dev/netcoder/specs/001-build-a-proof/contracts/openapi.yaml

Data Model: /Users/antbly/dev/netcoder/specs/001-build-a-proof/data-model.md

Research: /Users/antbly/dev/netcoder/specs/001-build-a-proof/research.md

Quickstart: /Users/antbly/dev/netcoder/specs/001-build-a-proof/quickstart.md

Repository roots:

- Backend: /Users/antbly/dev/netcoder/backend/CodeApi
- Frontend: /Users/antbly/dev/netcoder/frontend

Conventions:

- [P] = can run in parallel with other [P] tasks (different files, no dependency)
- TDD: write tests first where applicable; create minimal stubs as needed to compile tests

---

## Phase 3.1: Setup

- [x] T001 Backend dev CORS policy (localhost)

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/Program.cs
  - Actions:
    - Add permissive CORS policy for `http://localhost:4200` (Angular dev) and `http://localhost:5173` (Vite), enabled only in Development.
    - Map policy name to UseCors before MapControllers.

- [x] T002 Enforce 10s execution timeout (spec alignment)

  - Files:
    - /Users/antbly/dev/netcoder/backend/CodeApi/Services/CodeExecutionService.cs
    - /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_InfiniteLoop_TimesOut.cs
  - Actions:
    - Set TimeoutMs to 10_000 in service.
    - Update test assertion to expect ≈10_000 ms (e.g., Assert.InRange(durationMs, 9000, 14000)).

- [x] T003 Decide and align execution engine with research (Roslyn scripting)

  - Files:
    - /Users/antbly/dev/netcoder/backend/CodeApi/Services/CodeExecutionService.cs
    - /Users/antbly/dev/netcoder/backend/CodeApi/CodeApi.csproj
    - /Users/antbly/dev/netcoder/specs/001-build-a-proof/research.md
    - /Users/antbly/dev/netcoder/specs/001-build-a-proof/plan.md
  - Actions:
    - Refactor implementation to use Microsoft.CodeAnalysis.CSharp.Scripting per research decisions (capture stdout/stderr, enforce 10s timeout, map diagnostics with line/column anchored to user snippet).
    - Ensure packages Microsoft.CodeAnalysis.CSharp.Scripting and Microsoft.CodeAnalysis.Scripting.Common are referenced (already present) and remove unused external process logic.
    - If choosing to keep external process approach instead, update research.md and plan.md to reflect the actual approach and rationale.

- [x] T004 Frontend styling stack (Tailwind v4 + daisyUI 5)

  - Files/Dirs:
    - /Users/antbly/dev/netcoder/frontend/package.json
    - /Users/antbly/dev/netcoder/frontend/tailwind.config.ts (new)
    - /Users/antbly/dev/netcoder/frontend/src/styles.css
  - Actions:
    - Install Tailwind v4 and daisyUI; configure per `/.github/instructions/daisyui.instructions.md` and Angular guidelines in `/.github/instructions/angular.instructions.md`.
    - Apply base styles; ensure dark mode works.

- [x] T005 Frontend editor dependency (Monaco Editor)
  - Files:
    - /Users/antbly/dev/netcoder/frontend/package.json
    - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.ts
    - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.html
  - Actions:
    - Add monaco-editor (and types) dependency.
    - Integrate a basic Monaco editor instance for C# with minimal config; replace the textarea control.

---

## Phase 3.2: Tests First (must fail initially where behavior changes)

- [x] T006 [P] Contract test for POST /api/exec/run

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Contract/OpenApi_Contract_ExecRun.cs
  - Behavior: Assert response JSON has required fields per openapi.yaml (outcome, stdout, stderr, diagnostics[], durationMs, truncated).

- [x] T007 [P] Integration: Hello World success

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_HelloWorld_Succeeds.cs
  - Behavior: POST code Console.WriteLine("Hello, world!"); → outcome=Success, stdout contains Hello, world!, truncated=false.

- [x] T008 [P] Integration: Compile error diagnostics

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_SyntaxError_CompileError.cs
  - Behavior: POST invalid code; expect outcome=CompileError and diagnostics non-empty with approximate line/column.

- [x] T009 [P] Integration: Timeout at ~10s

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_InfiniteLoop_TimesOut.cs
  - Behavior: POST infinite wait; expect outcome=Timeout; durationMs ~ 10s (assert reasonable range).

- [x] T010 [P] Integration: Output truncation at 1 MB
  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_LargeOutput_Truncated.cs
  - Behavior: POST code writing >1 MB to stdout; expect truncated=true and stdout length ≤ 1 MB.

---

## Phase 3.3: Core Implementation (make tests pass)

- [x] T011 Service implementation aligned with research (Roslyn scripting)

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/Services/CodeExecutionService.cs
  - Actions:
    - Execute top-level C# via CSharpScript.RunAsync with cancellation.
    - Redirect Console.Out/Err to capture up to 1 MB; set truncated flag when exceeded.
    - Map Roslyn diagnostics (id, severity, message, 1-based line/column) anchored to the user snippet.

- [x] T012 Endpoint validations and logging

  - Files:
    - /Users/antbly/dev/netcoder/backend/CodeApi/Controllers/ExecController.cs
    - /Users/antbly/dev/netcoder/backend/CodeApi/Program.cs
  - Actions:
    - Ensure non-empty code and length ≤ 1 MB validations.
    - Log requestId, outcome, durationMs as structured fields.

- [x] T013 Dev CORS wiring
  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/Program.cs
  - Actions:
    - Apply the policy created in T001 via app.UseCors("dev").

---

## Phase 3.4: Integration

- [x] T014 [P] Developer HTTP examples

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/CodeApi.http
  - Actions: Add Hello World, Syntax Error, Timeout, Large Output examples with JSON bodies.

- [x] T015 Frontend API client: Angular Resource API

  - Files: /Users/antbly/dev/netcoder/frontend/src/app/services/api.service.ts
  - Actions:
    - Replace direct fetch with Angular Resource API per Angular 21 guidelines; define resource for POST /api/exec/run with types matching CodeSubmission/ExecutionResult.

- [x] T016 Frontend UI: Monaco editor and run flow
  - Files:
    - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.ts
    - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.html
    - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.css
  - Actions:
    - Swap textarea for Monaco; keep Run/Clear History controls, pending indicator, and results list.
    - Display stdout, stderr, outcome, durationMs, and truncated flag with clear styling (daisyUI components where appropriate).

---

## Phase 3.5: Polish

- [x] T017 [P] Docs polish and alignment

  - Files:
    - /Users/antbly/dev/netcoder/specs/001-build-a-proof/quickstart.md
    - /Users/antbly/dev/netcoder/README.md
    - /Users/antbly/dev/netcoder/specs/001-build-a-proof/research.md (if engine choice changed)
  - Actions:
    - Ensure quickstart reflects 10s timeout and sample curl commands.
    - Add internal-use warning (no sandbox) in README.
    - If approach changed, update research.md to explain rationale.

- [x] T018 [P] Performance smoke check
  - Files: N/A (manual doc entry in quickstart.md)
  - Actions: Measure p95 for Hello World end-to-end ≤ 2s on dev machine; record results.

---

## Dependencies & Ordering

- T001 → T013
- T002 → T009 (test assertion) and informs T011
- T003 → T011 (refactor) → T012
- Tests (T006–T010) precede core (T011–T013)
- Frontend (T015–T016) after backend endpoint stable (T012/T013)
- Docs/polish (T017–T018) after core/integration

## Parallel Execution Examples

These can run together safely (different files, no blocking dependency):

- Group A [P]: T006, T007, T008, T010
- Group B [P]: T004, T005, T014, T017, T018

Example commands (fish shell):

```fish
# Restore/build tests once
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet restore; cd -

# Run integration tests in parallel terminals/tabs
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test --filter FullyQualifiedName~Run_HelloWorld_Succeeds &
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test --filter FullyQualifiedName~Run_SyntaxError_CompileError &
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test --filter FullyQualifiedName~Run_InfiniteLoop_TimesOut &
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test --filter FullyQualifiedName~Run_LargeOutput_Truncated &
```

## Validation Checklist

- [x] All contracts have corresponding tests (contracts/openapi.yaml → T006)
- [x] Entities/tasks align with data model (CodeSubmission, Diagnostic, ExecutionResult)
- [x] Tests (T006–T010) come before implementation (T011–T013)
- [x] [P] tasks operate on different files
- [x] Each task lists exact file paths
- [x] Dev CORS configured for frontend ↔ backend in development
- [x] Testing frameworks align with Constitution (.NET → xUnit)

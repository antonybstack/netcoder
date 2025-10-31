# Tasks: Interactive C# 13 Code Execution PoC (Backend-first)

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
- TDD: write tests first, see Phase 3.2; create minimal stubs as needed to compile tests

---

## Phase 3.1: Setup

- [x] T001 Add Roslyn scripting packages to backend project

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/CodeApi.csproj
  - Actions:
    - Add Microsoft.CodeAnalysis.CSharp.Scripting (latest stable supporting C# 13)
    - Add Microsoft.CodeAnalysis.Scripting.Common (latest stable)

- [x] T002 [P] Create model: CodeSubmission

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/Models/CodeSubmission.cs
  - Actions:
    - Implement properties per data model: code (string, required, ≤ 1 MB), requestId (string, optional), submittedAt (DateTime assigned server-side)

- [x] T003 [P] Create model: Diagnostic + Severity enum

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/Models/Diagnostic.cs
  - Actions:
    - Implement properties per data model and enum Severity [Hidden, Info, Warning, Error]

- [x] T004 [P] Create model: ExecutionResult + Outcome enum

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/Models/ExecutionResult.cs
  - Actions:
    - Implement properties per data model and enum Outcome [Success, CompileError, RuntimeError, Timeout]

- [x] T005 [P] Controller stub to enable test compilation

  - Files:
    - /Users/antbly/dev/netcoder/backend/CodeApi/Controllers/ExecController.cs
    - /Users/antbly/dev/netcoder/backend/CodeApi/Program.cs
  - Actions:
    - Add [ApiController] at route "api/exec" with POST action "run" accepting CodeSubmission and returning ExecutionResult placeholder
    - Ensure Program.cs adds controllers and JSON options

- [x] T006 Scaffold backend tests project (xUnit + WebApplicationFactory)
  - Files/Dirs:
    - /Users/antbly/dev/netcoder/backend/CodeApi.Tests/CodeApi.Tests.csproj
    - /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/
    - /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Contract/
  - Actions:
    - Create xUnit project targeting net9.0
    - Add: xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, Microsoft.AspNetCore.Mvc.Testing
    - Reference /Users/antbly/dev/netcoder/backend/CodeApi/CodeApi.csproj

## Phase 3.2: Tests First (must fail initially)

- [x] T007 [P] Contract test for contracts/openapi.yaml

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Contract/OpenApi_Contract_ExecRun.cs
  - Behavior:
    - Start test server and call POST /api/exec/run with minimal valid JSON
    - Assert response JSON has required fields per openapi.yaml (outcome, stdout, stderr, diagnostics[], durationMs, truncated)

- [x] T008 [P] Integration: Hello World success

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_HelloWorld_Succeeds.cs
  - Behavior:
    - POST code: Console.WriteLine("Hello, world!")
    - Assert 200, outcome=Success, stdout contains "Hello, world!", truncated=false

- [x] T009 [P] Integration: Compile error diagnostics

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_SyntaxError_CompileError.cs
  - Behavior:
    - POST invalid code (e.g., missing semicolon)
    - Assert 200, outcome=CompileError, diagnostics non-empty

- [x] T010 [P] Integration: Timeout at 10s

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_InfiniteLoop_TimesOut.cs
  - Behavior:
    - POST infinite loop
    - Assert outcome=Timeout, durationMs ≥ 10000 (allow small variance)

- [x] T011 [P] Integration: Output truncation at 1 MB
  - Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_LargeOutput_Truncated.cs
  - Behavior:
    - POST code writing >1 MB to stdout
    - Assert truncated=true and stdout length ≤ 1 MB

## Phase 3.3: Core Implementation (make tests pass)

- [x] T012 Service interface: ICodeExecutionService

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/Services/ICodeExecutionService.cs
  - Actions:
    - Define: `Task<ExecutionResult> ExecuteAsync(string code, CancellationToken ct)`

- [x] T013 Service implementation: CodeExecutionService (Roslyn scripting)

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/Services/CodeExecutionService.cs
  - Behavior:
    - Execute top-level C# via scripting API
    - Capture stdout/stderr (StringWriter), enforce 10s timeout (CancellationToken + Task.WhenAny)
    - Truncate outputs at 1 MB and set truncated flag
    - Map diagnostics with id, severity, message, line, column

- [x] T014 Endpoint: wire DI, validations, and controller action
  - Files:
    - /Users/antbly/dev/netcoder/backend/CodeApi/Program.cs
    - /Users/antbly/dev/netcoder/backend/CodeApi/Controllers/ExecController.cs
  - Actions:
    - Register CodeExecutionService in DI
    - Validate request: non-empty code, length ≤ 1 MB; 400 on validation failure
    - Accept requestId; log correlation id with outcome and durationMs

## Phase 3.4: Integration

- [x] T015 [P] Developer HTTP examples

  - Files: /Users/antbly/dev/netcoder/backend/CodeApi/CodeApi.http
  - Actions:
    - Add POST /api/exec/run examples: Hello World, Syntax Error, Timeout, Large Output

- [x] T016 [P] Frontend API client (Angular Resource API)

  - Files: /Users/antbly/dev/netcoder/frontend/src/app/services/api.service.ts
  - Actions:
    - Define resource for POST /api/exec/run with types matching CodeSubmission/ExecutionResult

- [x] T017 [P] Frontend UI: code-page run flow
  - Files:
    - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.html
    - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.ts
    - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.css
  - Actions:
    - Add run button and pending state
    - Display stdout, stderr, outcome, durationMs, truncated
    - Maintain in-memory results list (newest first); Clear History action; allow concurrent runs

## Phase 3.5: Polish

- [x] T018 [P] Docs polish: verify quickstart and add curl samples

  - Files: /Users/antbly/dev/netcoder/specs/001-build-a-proof/quickstart.md

- [x] T019 [P] Repo warning: internal-only, no sandbox
  - Files: /Users/antbly/dev/netcoder/README.md

---

## Dependencies & Ordering

- T001 → T002, T003, T004, T005
- T002–T006 → T007–T011 [P] (tests should compile; controller stub may be minimal)
- T007–T011 → T012 → T013 → T014
- T014 → T015
- Frontend (T016–T017) after T014 (stable backend)
- Docs (T018–T019) anytime after backend stands up

## Parallel Execution Examples

These can run together safely (different files, no blocking dependency):

- Group A [P]: T002, T003, T004, T005
- Group B [P]: T008, T009, T010, T011
- Group C [P]: T015, T016, T017, T018, T019

Example commands (fish shell):

```fish
# Restore/build test projects once
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet restore; cd -

# Run integration tests in parallel terminals
# (launch multiple terminals/tabs or background jobs)
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test --filter FullyQualifiedName~Run_HelloWorld_Succeeds &
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test --filter FullyQualifiedName~Run_SyntaxError_CompileError &
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test --filter FullyQualifiedName~Run_InfiniteLoop_TimesOut &
cd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test --filter FullyQualifiedName~Run_LargeOutput_Truncated &
```

Task agent launch sketch (conceptual):

```text
agent:run T002 | agent:run T003 | agent:run T004 | agent:run T005
agent:run T008 | agent:run T009 | agent:run T010 | agent:run T011
```

## Validation Checklist

- All contracts have corresponding tests (contracts/openapi.yaml → T007)
- All entities have model tasks (CodeSubmission → T002; Diagnostic → T003; ExecutionResult → T004)
- Tests (T007–T011) precede implementation (T012–T014)
- [P] tasks operate on different files
- Each task lists absolute file paths
- Backend tests use xUnit per Constitution
- Dev examples provided in `CodeApi.http` and `quickstart.md`

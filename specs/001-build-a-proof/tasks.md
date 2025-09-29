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

- [P] = can run in parallel with other [P] tasks
- Use TDD where feasible: write tests before full implementation (controller/service stubs may be needed to compile tests)

---

T001. Backend setup: add Roslyn scripting packages

- Files: /Users/antbly/dev/netcoder/backend/CodeApi/CodeApi.csproj
- Actions:
  - Add package Microsoft.CodeAnalysis.CSharp.Scripting (latest stable supporting C# 13)
  - Add package Microsoft.CodeAnalysis.Scripting.Common (latest stable)
- Notes: Ensures server can compile/execute snippets via scripting API.

T002. Backend models: add DTOs per data-model.md [P]

- Files:
  - /Users/antbly/dev/netcoder/backend/CodeApi/Models/CodeSubmission.cs
  - /Users/antbly/dev/netcoder/backend/CodeApi/Models/Diagnostic.cs
  - /Users/antbly/dev/netcoder/backend/CodeApi/Models/ExecutionResult.cs
- Actions:
  - Implement properties exactly as in data-model.md (names/types)
  - Add enums for Outcome and Severity as needed

T003. Backend controller stub for contract compilation [P]

- Files:
  - /Users/antbly/dev/netcoder/backend/CodeApi/Controllers/ExecController.cs
  - /Users/antbly/dev/netcoder/backend/CodeApi/Program.cs
- Actions:
  - Create [ApiController] controller at route "api/exec"
  - Add POST action "run" that accepts CodeSubmission and returns ExecutionResult (placeholder NotImplemented)
  - Ensure Program.cs adds controllers and JSON options

T004. Backend tests project: scaffold xUnit and web testing infra

- Files/Dirs:
  - /Users/antbly/dev/netcoder/backend/CodeApi.Tests/CodeApi.Tests.csproj
  - /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/
  - /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Contract/
- Actions:
  - Create xUnit test project targeting net9.0
  - Add packages: xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, Microsoft.AspNetCore.Mvc.Testing
  - Reference CodeApi project

T005. Integration test: Hello World success [P]

- Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_HelloWorld_Succeeds.cs
- Behavior:
  - POST /api/exec/run with code: Console.WriteLine("Hello, world!")
  - Assert 200, outcome=Success, stdout contains "Hello, world!", truncated=false

T006. Integration test: Compile error diagnostics [P]

- Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_SyntaxError_CompileError.cs
- Behavior:
  - POST invalid code (e.g., missing semicolon)
  - Assert 200, outcome=CompileError, diagnostics non-empty

T007. Integration test: Timeout after 10s [P]

- Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_InfiniteLoop_TimesOut.cs
- Behavior:
  - POST code with infinite loop
  - Assert outcome=Timeout, durationMs >= 10000 (tolerate small variance)

T008. Integration test: Output truncation at 1 MB [P]

- Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Integration/Run_LargeOutput_Truncated.cs
- Behavior:
  - POST code that writes >1 MB to stdout
  - Assert truncated=true and stdout length <= 1 MB

T009. Contract test: Response shape matches openapi schema [P]

- Files: /Users/antbly/dev/netcoder/backend/CodeApi.Tests/Contract/Contract_ExecRun_ResponseShape.cs
- Behavior:
  - Call endpoint and validate JSON contains required fields per contracts/openapi.yaml

T010. Backend service interface: ICodeExecutionService

- Files: /Users/antbly/dev/netcoder/backend/CodeApi/Services/ICodeExecutionService.cs
- Actions:
  - Define `Task<ExecutionResult> ExecuteAsync(string code, CancellationToken ct)`

T011. Backend service implementation: CodeExecutionService (Roslyn scripting)

- Files: /Users/antbly/dev/netcoder/backend/CodeApi/Services/CodeExecutionService.cs
- Behavior:
  - Execute top-level C# code via scripting API
  - Capture stdout/stderr via StringWriter
  - Enforce 10s timeout using CancellationToken/Task.WhenAny
  - Truncate outputs at 1 MB and set truncated flag
  - Map compile/runtime diagnostics with message, id, severity, line/column

T012. Wire service + validations in controller

- Files:
  - /Users/antbly/dev/netcoder/backend/CodeApi/Program.cs
  - /Users/antbly/dev/netcoder/backend/CodeApi/Controllers/ExecController.cs
- Actions:
  - Register CodeExecutionService in DI
  - Validate: non-empty code, length <= 1 MB; return 400 on validation failure
  - Accept requestId; log correlation id with outcome and durationMs

T013. Backend dev UX: update CodeApi.http with example requests [P]

- Files: /Users/antbly/dev/netcoder/backend/CodeApi/CodeApi.http
- Actions:
  - Add POST /api/exec/run examples for Hello World, Syntax Error, Timeout

T014. Frontend API client using Angular Resource API [P]

- Files: /Users/antbly/dev/netcoder/frontend/src/app/services/api.service.ts (or new resource file)
- Actions:
  - Define resource for POST /api/exec/run
  - Types should match contracts ExecutionResult and CodeSubmission

T015. Frontend UI: code editor and run flow

- Files:
  - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.html
  - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.ts
  - /Users/antbly/dev/netcoder/frontend/src/app/components/code-page/code-page.css
- Actions:
  - Use Monaco Editor (latest) as the code editor; install/configure the `monaco-editor` package and basic C# language settings.
  - Run button; show pending state; display stdout/stderr, outcome, duration, truncation flag
  - Provide a "Clear History" action that empties the results list in the UI; history resets on page reload; no server-side persistence.
  - Display results as an unbounded session-scoped list (newest first).
  - Accessibility: announce result updates via ARIA live region

T016. Frontend wiring: connect Resource API to UI [P]

- Files: same as T014/T015
- Actions:
  - Submit code; prevent duplicate submit while pending
  - Support multiple results (concurrent runs) in UI list
  - Maintain an unbounded in-memory list of results for the current browser session (no cap); append results in completion order; clearing history must not cancel in-flight executions.

T017. Docs polish: verify quickstart and add curl samples [P]

- Files: /Users/antbly/dev/netcoder/specs/001-build-a-proof/quickstart.md
- Actions:
  - Confirm instructions run as-is against local backend

T018. Risk note: internal-only, no sandbox [P]

- Files: /Users/antbly/dev/netcoder/README.md
- Actions:
  - Add prominent warning that PoC executes arbitrary code; for trusted internal usage only

---

Dependencies & Ordering

- T001 → T002/T003
- T002/T003 → T004
- T004 → T005–T009 [P]
- T005–T009 → T010–T012 (make tests pass)
- T012 → T013
- Frontend (T014–T016) can proceed after backend endpoint stable
- Docs (T017–T018) anytime after backend stands up

Parallel Execution Guidance

- Group 1 [P]: T002, T003
- Group 2 [P]: T005, T006, T007, T008, T009
- Group 3 [P]: T013, T014, T016, T017, T018

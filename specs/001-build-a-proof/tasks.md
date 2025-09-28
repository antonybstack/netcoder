# Tasks: POC – Web code editor that runs user C# code on a server and returns console output

**Input**: Design documents from `/specs/001-build-a-proof/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)

```text
1. Load plan.md from feature directory
2. Load optional design documents (data-model, contracts, research, quickstart)
3. Generate tasks by category (Setup, Tests, Core, Integration, Polish)
4. Apply task rules (parallelization, TDD ordering)
5. Number tasks sequentially (T001, T002...)
6. Create dependency notes and parallel examples
7. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Phase 3.1: Setup

- [ ] T001 Create backend project structure at `/Users/antbly/dev/netcoder/backend/src/Api` with folders: `Endpoints/`, `Services/`, `Models/`, `Observability/`; add solution and test dirs at `/Users/antbly/dev/netcoder/backend/tests/{Contract,Integration,Unit}`.
- [ ] T002 Initialize .NET 10 minimal API in `/Users/antbly/dev/netcoder/backend/src/Api/Program.cs` with OpenAPI, CORS (allow <http://localhost:4200> in dev), `/health` endpoint, and metrics stub.
- [ ] T003 [P] Add backend observability: request logging with correlation ID (X-Request-ID), basic metrics counters/histogram scaffolding in `/Users/antbly/dev/netcoder/backend/src/Api/Observability/`.
- [ ] T004 [P] Scaffold backend models per spec in `/Users/antbly/dev/netcoder/backend/src/Api/Models/` for `CodeSubmission.cs`, `RunResult.cs`, `Diagnostic.cs`, `ExceptionInfo.cs`.
- [ ] T005 Configure environment files: create `/Users/antbly/dev/netcoder/docker-compose.dev.yml` and `/Users/antbly/dev/netcoder/docker-compose.prod.yml` mapping backend port 5080 and frontend 4200 (no conflicts); add `/Users/antbly/dev/netcoder/.env.example` documenting variables.
- [ ] T006 Frontend setup: ensure Tailwind v4 and daisyUI 5 are active by editing `/Users/antbly/dev/netcoder/app/src/styles.css` to contain `@import "tailwindcss";` and `@plugin "daisyui";` (retain existing styles) and add dark-first base classes if needed.
- [ ] T007 [P] Frontend dependencies: add Monaco Editor integration files under `/Users/antbly/dev/netcoder/app/src/app/components/editor/` and prepare a provider for Angular zoneless/Signals usage.

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3

- [ ] T008 [P] Contract test for POST `/api/run` using OpenAPI at `/Users/antbly/dev/netcoder/specs/001-build-a-proof/contracts/run.openapi.yaml`; create `/Users/antbly/dev/netcoder/backend/tests/Contract/RunEndpointContractTests.cs` validating schema and required fields.
- [ ] T009 [P] Backend unit tests for `CodeRunner` constraints in `/Users/antbly/dev/netcoder/backend/tests/Unit/CodeRunnerLimitsTests.cs` (timeout 10s, memory 256MB, output cap 1MB, stdin not supported, no network/process spawn).
- [ ] T010 [P] Frontend integration test for Run flow in `/Users/antbly/dev/netcoder/app/tests/integration/run-flow.spec.ts`: editor loads sample, clicking Run displays "Hello, world!".
- [ ] T011 [P] Backend integration test in `/Users/antbly/dev/netcoder/backend/tests/Integration/RunHelloWorldTests.cs`: POST with `Console.WriteLine("Hello, world!");` returns expected stdout and success.

## Phase 3.3: Core Implementation (ONLY after tests are failing)

- [ ] T012 Implement entity models from data-model in `/Users/antbly/dev/netcoder/backend/src/Api/Models/*.cs` (align fields: CodeSubmission, RunResult, Diagnostic, ExceptionInfo).
- [ ] T013 Implement `CodeRunner` service in `/Users/antbly/dev/netcoder/backend/src/Api/Services/CodeRunner.cs` to compile C# 13 with Roslyn and execute in isolated process with: 10s timeout, 256MB memory cap, 1MB combined stdout+stderr cap, disallow stdin/network/process spawn, allow temp-dir file I/O, return standardized errors.
- [ ] T014 Implement POST `/api/run` endpoint in `/Users/antbly/dev/netcoder/backend/src/Api/Endpoints/RunEndpoint.cs` wiring validation, invocation of `CodeRunner`, mapping to `RunResult`, and status codes (200/400/429/500).
- [ ] T015 Add rate limiting (5 runs/min per IP, burst 10) middleware/policy in `/Users/antbly/dev/netcoder/backend/src/Api/Program.cs` with 429 response including Retry-After.
- [ ] T016 Add correlation ID middleware and metrics emission on each run (success/phase/duration) under `/Users/antbly/dev/netcoder/backend/src/Api/Observability/` and register in `Program.cs`.
- [ ] T017 [P] Frontend service `/Users/antbly/dev/netcoder/app/src/app/services/run.service.ts` with typed request/response per `RunResult` and `CodeSubmission`, using Signals for status and result.
- [ ] T018 Frontend editor component: create `/Users/antbly/dev/netcoder/app/src/app/components/editor/editor.component.ts|html|css` with Monaco editor, Run and Reset buttons (daisyUI), output panel tabs, and state rendering for success/errors/timeout/truncation.
- [ ] T019 [P] Wire app shell to include the editor component and basic navbar using daisyUI in `/Users/antbly/dev/netcoder/app/src/app/app.html` and styles.

## Phase 3.4: Integration

- [ ] T020 Enable CORS in backend for dev origin <http://localhost:4200> and document in `/Users/antbly/dev/netcoder/specs/001-build-a-proof/quickstart.md`.
- [ ] T021 Expose `/health` and `/metrics` endpoints; ensure they are excluded from rate limiting; document in quickstart.
- [ ] T022 Validate quickstart: run curl example end-to-end; update examples if fields changed.
- [ ] T023 [P] Update `.github/copilot-instructions.md` if new frameworks/tools added during implementation.

## Phase 3.5: Polish

- [ ] T024 [P] Add unit tests for error mapping and diagnostics formatting in `/Users/antbly/dev/netcoder/backend/tests/Unit/RunResultFormattingTests.cs`.
- [ ] T025 Add frontend unit tests for UI state components (timeout, oom, truncation badges) under `/Users/antbly/dev/netcoder/app/src/app/components/editor/editor.component.spec.ts`.
- [ ] T026 Performance smoke: ensure p95 compile+run for Hello World ≤ 3s on dev; log results; adjust limits if needed.
- [ ] T027 [P] Documentation: finalize `/Users/antbly/dev/netcoder/specs/001-build-a-proof/quickstart.md` and add `README` snippets linking API contract and local run.
- [ ] T028 Reduce duplication and ensure consistent types across client/server (e.g., shared enum strings).

## Dependencies

- T008–T011 (tests) MUST run and fail before T012–T019 implementation.
- T012 models unblock T013 service and T014 endpoint.
- T013 service required by T014 endpoint.
- T017 frontend service should precede T018 editor wiring.
- T016 observability can be added anytime after Program.cs; ensure registration before endpoint tests.
- T020–T023 integration after core implementation.
- Polish (T024–T028) after core and integration pass.

## Parallel Example

```text
# Launch independent tests in parallel
Task: "T008 Contract test /api/run in backend/tests/Contract/RunEndpointContractTests.cs"
Task: "T009 Unit tests CodeRunner limits in backend/tests/Unit/CodeRunnerLimitsTests.cs"
Task: "T010 Frontend integration test run-flow in app/tests/integration/run-flow.spec.ts"
Task: "T011 Backend integration hello-world in backend/tests/Integration/RunHelloWorldTests.cs"

# Parallel core tasks on different files
Task: "T017 [P] run.service.ts"
Task: "T019 [P] app shell wiring"
```

## Validation Checklist

- [ ] All contracts have corresponding tests (run.openapi.yaml → T008)
- [ ] All entities have model tasks (CodeSubmission, RunResult, Diagnostic, ExceptionInfo → T012/T004)
- [ ] All tests come before implementation (T008–T011 before T012–T019)
- [ ] Parallel tasks truly independent (marked [P])
- [ ] Each task specifies exact file path
- [ ] No task modifies same file as another [P] task
- [ ] Environment-specific deployment configuration provided and port collision avoidance documented (T005)

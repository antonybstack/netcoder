# Implementation Plan: Realtime C# Intellisense & Syntax Feedback in Editor

**Branch**: `001-frontend-client-receive` | **Date**: 2025-11-22 | **Spec**: `/Users/antbly/dev/netcoder/specs/001-frontend-client-receive/spec.md`
**Input**: Feature specification from `/specs/001-frontend-client-receive/spec.md`

## Execution Flow (/plan command scope)

```text
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code or `AGENTS.md` for opencode).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:

- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary

- Primary: Provide real-time completions, diagnostics, hover, and signature help for C# 14 targeting .NET 10 in the in-browser editor.
- Scope: Single-file context, anonymous access, best-effort responsiveness for very large files.
- Approach: Backend uses .NET 10 SignalR; frontend uses `@microsoft/signalr` with Angular 21 zoneless and Signals-based async patterns (`signal`, `computed`, `effect`, `resource`, `httpResource`). Disconnects show status, keep local syntax-only, auto-retry.

## Technical Context

**Language/Version**: Frontend Angular 21 (zoneless), Backend .NET 10 (C# 14).  
**Primary Dependencies**: Backend SignalR; Frontend `@microsoft/signalr`, Angular Signals API (`signal`, `computed`, `effect`), Angular Resource API (`resource`, `httpResource`).  
**Storage**: N/A for this feature.  
**Testing**: Backend xUnit integration/contract tests; no frontend tests required per constitution.  
**Target Platform**: Web frontend + containerized backend service.  
**Project Type**: web (frontend + backend).  
**Performance Goals**: p95 time-to-first-completion ≤ 200 ms; p95 diagnostic refresh ≤ 300 ms; responsive typing feel.  
**Constraints**: Single-file editing context; anonymous access; best-effort large-file responsiveness; degrade to local syntax-only during disconnect with auto-retry.  
**Scale/Scope**: Single-user interactive editing sessions; concurrent sessions not constrained in MVP.

## Constitution Check

_GATE: Must pass before Phase 0 research. Re-check after Phase 1 design._

The plan MUST include a short, explicit Constitution Check addressing how the proposal
conforms to each relevant principle in `/.specify/memory/constitution.md`. At minimum the
following gates apply:

- Runtime & Networking: Document expected network flows; justify any external outbound
  calls; show how payloads are minimised or batched where appropriate. Internal
  non-HTTPS operation is acceptable only when the plan explains TLS termination at the
  edge (Cloudflare Tunnel or equivalent) and describes request provenance validation.
- Environment configuration: Document separate production and development environment
  configurations (ports, host bindings, docker-compose overrides or env files). Plans
  MUST show how development deployments avoid port/host collisions with production and
  CI, and how health/metrics/admin endpoints are separated or namespaced.
- Technology Alignment: If the project includes frontend or backend components, the
  plan MUST state how it aligns with the mandated stack (Frontend: Angular 21 zoneless,
  Tailwind v4; Backend: .NET 10) or justify deviations.
- Frontend HTTP API handling (Angular only): Plans MUST use Angular's Resource API for
  HTTP/data fetching. RxJS-based HttpClient patterns are PROHIBITED for API handling
  unless explicitly justified with a rationale and migration plan.
- Tests & Quality Gates: Confirm unit and integration tests and any required contract
  tests; performance testing is OPTIONAL and only required when acceptance criteria
  specify performance targets. Testing frameworks MUST align with the stack:
  .NET → xUnit with Microsoft/.NET native test runner/packages.
- Always‑Green Build: Plans MUST explain how the team keeps branches green and what
  local/CI commands are run regularly (e.g., `dotnet build`, `dotnet test`, `ng build`,
  `ng test` if frontend tests exist). Merges MUST be gated by CI build/test.
- Observability: Describe logging (structured logs and correlation IDs) and metrics
  exposure for the feature surface.
- Styling & UX: Frontend plans MUST adopt dark-mode-first Tailwind v4 styling or
  document a strong rationale for an alternative.
- Scaffolding & Code Generation: Prefer official CLI tools (e.g., `ng generate`,
  `dotnet new`) for generating templates/boilerplate rather than hand-written scaffolds.

[Gates determined based on constitution file]

Decision and conformance summary:

- Runtime & Networking: SignalR over WebSockets; no external outbound calls. Internal non-HTTPS acceptable; TLS termination handled at edge (e.g., Cloudflare). Payloads are minimal JSON messages with debounced updates.
- Environment configuration: Dev vs prod documented in quickstart; separate host/ports; no collisions with CI. Health/metrics endpoints follow backend conventions.
- Technology Alignment: Frontend Angular 21 zoneless + Signals; Tailwind/daisyUI maintained. Backend .NET 10 with controllers and SignalR. Aligns with constitution.
- Frontend HTTP handling: For HTTP, use Angular Resource API. SignalR real-time channel complements HTTP; no RxJS for HTTP flows.
- Tests & Quality Gates: Backend integration/contract tests (xUnit). Frontend tests not required. Always-green: `dotnet build`, `dotnet test`, `ng build` (if needed), CI gates merges.
- Observability: Structured logs with correlation ID on backend; client logs with levels. Metrics: connection counts, message rates, latency histograms.
- Styling & UX: Tailwind v4 + daisyUI; dark-mode-first.
- Scaffolding & Code Generation: Use `dotnet new` and `ng generate` as applicable.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)

<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
backend/
├── CodeApi/
│   ├── Controllers/
│   ├── Services/
│   ├── Program.cs
│   └── CodeApi.csproj
└── CodeApi.Tests/
  ├── Contract/
  └── Integration/

frontend/
├── src/
│   ├── app/
│   │   ├── components/code-page/
│   │   └── services/
│   ├── main.ts
│   └── styles.css
└── angular.json
```

**Structure Decision**: Web application (frontend + backend) using existing repo layout shown above.

## Phase 0: Outline & Research

1. **Extract unknowns from Technical Context** above:

   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

1. **Generate and dispatch research agents**:

```text
For each unknown in Technical Context:
  Task: "Research {unknown} for {feature context}"
For each technology choice:
  Task: "Find best practices for {tech} in {domain}"
```

1. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts

Prerequisite: research.md complete

1. **Extract entities from feature spec** → `data-model.md`:

   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

1. **Generate API contracts** from functional requirements:

   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

1. **Generate contract tests** from contracts:

   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

1. **Extract test scenarios** from user stories:

   - Each story → integration test scenario
   - Quickstart test = story validation steps

1. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/bash/update-agent-context.sh copilot`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/\*, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach

This section describes what the /tasks command will do - DO NOT execute during /plan

**Task Generation Strategy**:

- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each contract → contract test task [P]
- Each entity → model creation task [P]
- Each user story → integration test task
- Implementation tasks to make tests pass

**Ordering Strategy**:

- TDD order: Tests before implementation
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation

These phases are beyond the scope of the /plan command

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking

Fill ONLY if Constitution Check has violations that must be justified

| Violation                  | Why Needed         | Simpler Alternative Rejected Because |
| -------------------------- | ------------------ | ------------------------------------ |
| [e.g., 4th project]        | [current need]     | [why 3 projects insufficient]        |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient]  |

## Progress Tracking

This checklist is updated during execution flow

**Phase Status**:

- [x] Phase 0: Research complete (/plan command)
- [x] Phase 1: Design complete (/plan command)
- [ ] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:

- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---

_Based on Constitution v1.3.0 - See `/.specify/memory/constitution.md`_

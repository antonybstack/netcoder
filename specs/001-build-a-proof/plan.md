# Implementation Plan: POC – Web code editor that runs user C# code on a server and returns console output

**Branch**: `001-build-a-proof` | **Date**: 2025-09-27 | **Spec**: /Users/antbly/dev/netcoder/specs/001-build-a-proof/spec.md
**Input**: Feature specification from `/specs/001-build-a-proof/spec.md`

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
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

## Summary

A web-based C# playground: users write C# 13 (top‑level statements allowed) in a browser editor and click Run. The client sends the code to a backend API that compiles and executes it in a constrained sandbox and returns stdout/stderr or errors. The POC enforces: 10s execution timeout, 256 MB memory cap, 1 MB combined output cap, no stdin, moderate sandbox (temp‑dir file I/O only; no network; no process spawn).

High‑level approach:

- Frontend (existing Angular app folder) integrates a code editor (Monaco) with a Run action, renders result states (success, compile error, runtime error, timeout, input-not-supported, OOM, truncated output).
- Backend (.NET 10 minimal API) exposes POST /api/run to compile and execute C# in an isolated process/domain with resource/time/output limits, returning a structured result.

## Technical Context

**Language/Version**: Frontend Angular 20 (zoneless); Backend .NET 10 minimal APIs; User code C# 13  
**Primary Dependencies**: Angular Signals; Monaco Editor; Tailwind CSS v4 (dark-first) + daisyUI 5; .NET Roslyn (C# compile/execute); Structured logging (Serilog or built-in)  
**Storage**: N/A (stateless POC)  
**Testing**: Unit tests (backend services/utilities); Contract tests for POST /api/run; Frontend integration test for Run flow  
**Target Platform**: Containers (Docker) and local dev; Edge TLS termination (e.g., Cloudflare)  
**Project Type**: web (frontend + backend)  
**Performance Goals**: p95 compile+run for Hello World ≤ 3s on dev machine  
**Constraints**: Execution timeout 10s; Memory limit 256 MB; Output limit 1 MB combined; CPU limit 1 vCPU (time-sliced)  
**Scale/Scope**: Developer demo; single-file submission; no persistence; basic abuse controls (5 runs/min/IP burst 10)

## Constitution Check

- Runtime & Networking: Single internal call from frontend → backend; no external outbound calls. Payload minimal (source string only). Responses capped at 1 MB. OK.
- Environment configuration: Separate dev/prod documented. Dev: frontend <http://localhost:4200>, backend <http://localhost:5080>. Compose overrides recommended; host port collisions avoided. Health/metrics endpoints separated via path (`/health`, `/metrics`). OK.
- Technology Alignment: Frontend adopts Angular 20 zoneless + Tailwind v4 dark‑first; Backend targets .NET 10 minimal APIs. Note: Spec mentioned .NET 9; plan aligns to .NET 10 per Constitution with no functional impact. OK.
- Observability: Structured logs with correlation ID (X-Request-ID generated if missing). Minimal metrics (request count, duration, run outcomes). OK.
- Styling & UX: Tailwind v4 dark-first; daisyUI 5 for components. OK.
- Tests & Quality Gates: Contract tests for /api/run; unit tests; optional perf smoke. OK.

Result: Initial Constitution Check PASS

## Project Structure

### Documentation (this feature)

```text
specs/001-build-a-proof/
├── plan.md              # This file (/plan output)
├── research.md          # Phase 0 output (/plan)
├── data-model.md        # Phase 1 output (/plan)
├── quickstart.md        # Phase 1 output (/plan)
├── contracts/           # Phase 1 output (/plan)
└── tasks.md             # Phase 2 output (/tasks)
```

### Source Code (repository root)

```text
backend/                   # New .NET 10 minimal API project (to be created)
├── src/
│   └── Api/
│       ├── Program.cs
│       ├── Endpoints/RunEndpoint.cs
│       ├── Services/CodeRunner.cs
│       ├── Models/
│       └── Observability/
└── tests/
    ├── Contract/
    ├── Integration/
    └── Unit/

app/                       # Existing Angular frontend app (Angular 20 target)
├── src/
│   ├── app/
│   │   ├── components/editor/
│   │   ├── services/run.service.ts
│   │   └── app.*
│   ├── styles.css         # Tailwind v4 + daisyUI 5
│   └── index.html
└── tests/
    └── integration/
```

**Structure Decision**: Web application with separate frontend (`/app`) and backend (`/backend`) projects, documented above.

## Phase 0: Outline & Research

1. Unknowns extracted and resolved in research.md:

   - Backend execution strategy for C# (Roslyn compile vs scripting) → choose in-memory compile with isolated process and constrained resources.
   - Rate limiting policy → 5 runs/min per IP, burst 10; 429 on exceed.
   - CPU limiting approach → 1 vCPU time-slice; kill on hard breach.
   - Editor integration details in Angular → Monaco Editor; keyboard shortcuts; basic C# syntax highlighting.
   - Observability details → correlation IDs, minimal metrics; log redaction of source (hash only) to avoid sensitive leakage.

2. Research tasks dispatched (documented in research.md) and consolidated decisions recorded with rationale and alternatives.

**Output**: research.md (completed)

## Phase 1: Design & Contracts

1. Entities defined in data-model.md from spec (CodeSubmission, RunResult) with validation rules and state outcomes.
2. API contract defined in contracts/run.openapi.yaml for POST /api/run with request/response schemas, errors (400/429/500) and examples.
3. Contract tests planned (to be created during /tasks) for schema validation; backend unit tests planned for CodeRunner constraints and formatting.
4. Quickstart created for local dev: ports, commands, curl example, environment hints.
5. Agent context updated for Copilot via `.specify/scripts/bash/update-agent-context.sh copilot`.

**Output**: data-model.md, contracts/run.openapi.yaml, quickstart.md

## Phase 2: Task Planning Approach

- Strategy: generate tasks from contracts and data model; TDD ordering; mark parallelizable tasks.
- Ordering: backend contracts/tests → backend implementation → frontend service → editor component → integration test → polish/observability.
- Estimated tasks: ~25–30.

## Phase 3+: Future Implementation

- Phase 3: /tasks to generate tasks.md
- Phase 4: Implement, make tests pass
- Phase 5: Validate quickstart, smoke perf

## Complexity Tracking

| Violation                         | Why Needed              | Simpler Alternative Rejected Because                                              |
| --------------------------------- | ----------------------- | --------------------------------------------------------------------------------- |
| Spec referenced .NET 9 vs .NET 10 | Align with Constitution | Upgrade is trivial and removes future migration; no functional diffs for this POC |

## Progress Tracking

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
- [x] Complexity deviations documented

---

_Based on Constitution v1.1.0 - See `/memory/constitution.md`_

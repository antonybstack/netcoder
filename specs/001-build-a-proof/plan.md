# Implementation Plan: Interactive C# 13 Code Execution PoC (Backend-first)

**Branch**: `001-build-a-proof` | **Date**: 2025-09-28 | **Spec**: specs/001-build-a-proof/spec.md  
**Input**: Feature specification from `/specs/001-build-a-proof/spec.md`

## Execution Flow (/plan command scope)

```
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

Enable a PoC where users type short C# 13 snippets in a browser and execute them on a backend, returning stdout/stderr and diagnostics. Backend-first approach: provide a single HTTP endpoint that accepts code and returns an execution result. For the PoC, no sandbox and internal-only. Constraints: 10s timeout, 1 MB max code, 1 MB max output, unlimited per-client concurrency.

## Technical Context

**Language/Version**: Backend: C# 13 on .NET 9 (current repo); Frontend: Angular 20  
**Primary Dependencies**: Roslyn C# Scripting (Microsoft.CodeAnalysis.CSharp.Scripting); Angular Resource API for HTTP; Tailwind v4 + daisyUI for UI  
**Storage**: N/A (no persistence in PoC)  
**Testing**: Backend: xUnit integration/contract tests
**Target Platform**: Local development (macOS/Linux/Windows); containerised future deployment  
**Project Type**: Web (frontend + backend)  
**Performance Goals**: Hello World end-to-end ≤ 2s p95 on dev machine  
**Constraints**: Execution timeout = 10s; submission size ≤ 1 MB; output limit = 1 MB; per-client concurrency unlimited; no sandbox (internal-only)  
**Scale/Scope**: Internal PoC; not for public/untrusted access

## Constitution Check

- Runtime & Networking: Internal HTTP between frontend and backend only; no outbound external calls. For production, TLS terminates at edge (e.g., Cloudflare Tunnel). Payloads are single JSON request/response; no batching needed.
- Environment configuration: Development uses launch profile binding to <http://localhost:5189> (backend). Production to be containerised with separate host port mappings; health/metrics endpoints to be namespaced.
- Technology Alignment: Frontend will use Angular 20 zoneless and Resource API for HTTP; Tailwind v4/daisyUI for styling. Backend uses .NET 9.
- Frontend HTTP API handling: Will use Angular Resource API; RxJS HttpClient patterns prohibited.
- Tests & Quality Gates: Plan includes backend xUnit integration and contract tests. Performance testing not required beyond stated goal.
- Observability: Structured logs with correlation ID (requestId), durationMs, outcome; minimal metrics surface planned for later.
- Styling & UX: Dark-mode-first Tailwind v4 with daisyUI components for the editor page.
- Scaffolding & Code Generation: Use `dotnet new` and `ng generate` where new scaffolds are needed; reuse existing projects.

Gate Evaluation: Initial Constitution Check PASS (with documented backend version deviation and migration plan).

## Project Structure

### Documentation (this feature)

```text
specs/001-build-a-proof/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
    └── openapi.yaml
```

### Source Code (repository root)

```text
backend/
└── CodeApi/
    ├── Controllers/
    │   └── SampleController.cs
    ├── Program.cs
    ├── Properties/
    │   └── launchSettings.json
    └── CodeApi.csproj

frontend/
├── angular.json
└── src/
    ├── app/
    │   ├── components/
    │   │   └── code-page/
    │   │       ├── code-page.ts
    │   │       ├── code-page.html
    │   │       └── code-page.css
    │   └── services/
    │       └── api.service.ts
    └── main.ts
```

**Structure Decision**: Web application (frontend + backend) using existing `backend/CodeApi` and `frontend/` projects.

## Phase 0: Outline & Research

All known unknowns identified; decisions and rationale recorded in `research.md`:

- Execution engine: Roslyn C# scripting (chosen)
- Output capture and truncation (chosen)
- Timeout enforcement (chosen)
- Concurrency & security posture (chosen)

## Phase 1: Design & Contracts

- Data model defined in `data-model.md`
- API contract defined in `contracts/openapi.yaml` (POST /api/exec/run)
- Quickstart steps defined in `quickstart.md`
- Agent context updated via `.specify/scripts/bash/update-agent-context.sh copilot`

Re-evaluated Constitution Check: PASS (no new violations).

## Phase 2: Task Planning Approach

- Generate tasks from contracts and data model using `/tasks` command and tasks template.
- TDD ordering: contract and integration tests first, then implementation to pass tests.
- Parallelise independent work (contracts, backend execution service wiring, frontend UI shell).

## Phase 3+: Future Implementation

Unchanged from template.

## Complexity Tracking

| Violation                   | Why Needed                           | Simpler Alternative Rejected Because          |
| --------------------------- | ------------------------------------ | --------------------------------------------- |
| Backend uses .NET 9 (vs 10) | Aligns with existing repo; PoC speed | Immediate migration adds time without benefit |

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
- [ ] All NEEDS CLARIFICATION resolved
- [x] Complexity deviations documented

---

_Based on Constitution v1.2.0 - See `/.specify/memory/constitution.md`_

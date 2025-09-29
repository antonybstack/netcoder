# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)

```
1. Load plan.md from feature directory
   → If not found: ERROR "No implementation plan found"
   → Extract: tech stack, libraries, structure
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks
   → contracts/: Each file → contract test task
   → research.md: Extract decisions → setup tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting
   → Tests: contract tests, integration tests
   → Core: models, services, CLI commands
   → Integration: DB, middleware, logging
   → Polish: unit tests, performance, docs
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have models?
   → All endpoints implemented?
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `src/`, `tests/` at repository root
- **Web app**: `backend/src/`, `frontend/src/`
- **Mobile**: `api/src/`, `ios/src/` or `android/src/`
- Paths shown below assume single project - adjust based on plan.md structure

## Phase 3.1: Setup

- [ ] T001 Create project structure per implementation plan
- [ ] T002 Initialize [language] project with [framework] dependencies
- [ ] T003 [P] Configure linting and formatting tools
- [ ] T004 Prefer official CLI scaffolding for boilerplate (e.g., `ng generate`, `dotnet new`). Record exact commands in the plan.

**Environment & Deployment Setup**

- [ ] Create environment-specific deployment files (e.g., `docker-compose.dev.yml`,
      `docker-compose.prod.yml`) and document host port mappings for development to
      avoid conflicts with production and CI.
- [ ] Provide example `.env.example` and document required environment variables for
      each environment (development, staging, production).
- [ ] Ensure CI/CD pipelines use non-conflicting port ranges and do not deploy
      development stacks to production clusters.

**Testing Setup (align with Constitution)**

- [ ] If backend = .NET: create xUnit test projects (e.g., `dotnet new xunit`), reference SUT projects, and enable Microsoft/.NET native test runner in CI.

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3

**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**

- [ ] T100 [P] Contract tests for each endpoint (one file per endpoint under tests/contract)
- [ ] T101 [P] Integration tests for primary flows (tests/integration)
- [ ] T102 [P] Backend integration tests (xUnit) scaffolds

## Phase 3.3: Core Implementation (ONLY after tests are failing)

- [ ] T200 [P] Models/entities
- [ ] T201 [P] Services/business logic
- [ ] T202 [P] CLI/commands or endpoints
- [ ] T203 Input validation
- [ ] T204 Error handling and logging

## Phase 3.4: Integration

- [ ] T300 Connect services to infrastructure (DB, queues, etc.)
- [ ] T301 Auth middleware
- [ ] T302 Request/response logging
- [ ] T303 CORS and security headers

## Phase 3.5: Polish

- [ ] T400 [P] Unit tests for validation
- [ ] T401 Performance tests (OPTIONAL - only when acceptance criteria require performance targets)
- [ ] T402 [P] Update docs (api.md, quickstart.md)
- [ ] T403 Remove duplication
- [ ] T404 Manual validation per quickstart.md

## Dependencies

- Tests (T100-T102) before implementation (T200-T204)
- T200 blocks T201, T300
- T301 blocks T303
- Implementation before polish (T400-T404)

## Parallel Example

```
# Launch T100-T102 together (different files):
Task: "Contract tests for endpoints in tests/contract/*"
Task: "Integration tests in tests/integration/*"
Task: "Backend unit tests (xUnit)"
```

## Notes

- [P] tasks = different files, no dependencies
- Verify tests fail before implementing
- Commit after each task
- Avoid: vague tasks, same file conflicts

## Task Generation Rules

_Applied during main() execution_

1. **From Contracts**:
   - Each contract file → contract test task [P]
   - Each endpoint → implementation task
2. **From Data Model**:
   - Each entity → model creation task [P]
   - Relationships → service layer tasks
3. **From User Stories**:

   - Each story → integration test [P]
   - Quickstart scenarios → validation tasks

4. **Ordering**:
   - Setup → Tests → Models → Services → Endpoints → Polish
   - Dependencies block parallel execution

## Validation Checklist

_GATE: Checked by main() before returning_

- [ ] All contracts have corresponding tests
- [ ] All entities have model tasks
- [ ] All tests come before implementation
- [ ] Parallel tasks truly independent
- [ ] Each task specifies exact file path
- [ ] No task modifies same file as another [P] task
- [ ] Environment-specific deployment configuration provided (dev/prod) and port
      collision avoidance documented
- [ ] Testing frameworks align with Constitution (.NET → xUnit)

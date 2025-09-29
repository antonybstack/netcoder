<!--
Sync Impact Report
- Version change: 1.1.0 → 1.2.0
- Modified principles / sections:
  - Frontend (Angular 20 & Signals): HTTP Resource API is REQUIRED; RxJS-based HttpClient patterns are prohibited for API handling unless justified in the plan's Constitution Check.
  - Development Workflow & Quality Gates: Added explicit testing frameworks (xUnit for .NET) and CLI scaffolding policy.
- Added sections: none
- Removed sections: none
- Templates requiring updates:
  - .specify/templates/plan-template.md ✅ updated (gates: Resource API, test frameworks, CLI scaffolding)
  - .specify/templates/tasks-template.md ✅ updated (setup tasks for xUnit; CLI scaffolding)
  - .specify/templates/spec-template.md ✅ no change required
-->

# Netcoder Constitution

## Core Principles

### 1. General

- The project is CONTAINER-FIRST and MAY operate without embedded HTTPS/TLS inside containers.
- TLS/SSL termination and public-facing security will be handled by external networking infrastructure (e.g., Cloudflare Tunnel, edge proxy).
- The project prioritises integration and end-to-end testing over unit tests.

### 2. Frontend: Angular 20

- Frontend applications MUST use Angular 20 in zoneless mode (avoids zone.js).'
- The Signals API MUST be used for state management.
- For HTTP and data fetching, the Angular Resource API (for example, httpResource built on HttpClient) MUST be used so that
- request state and responses are available as Signals.
- RxJS-based patterns are PROHIBITED unless a clear, documented justification is provided in the plan's Constitution Check.
- Styling MUST use [tailwind-css](https://www.npmjs.com/package/tailwindcss), ([daisyUI](https://npmjs.com/package/daisyui)), with a dark-mode-first approach.
- The project MAY adopt well-maintained open-source Tailwind libraries to accelerate UI development, such as [tailwind-css/forms](https://npmjs.com/package/@tailwindcss/forms).
- Project structure and build MUST follow Angular best practices and conventions.

#### DO

- DO Use Angular 20 for building web applications.
- DO Use Signals API for state management.
- DO Use Angular Resource API (e.g., httpResource built on HttpClient) for HTTP and data fetching.
- DO Use Tailwind CSS with a dark-mode-first approach for styling.
- DO Use barrel files (index.ts) to simplify and centralize imports.
- DO Use Dependency Injection (DI) for managing service dependencies.
- DO Use Angular CLI for project scaffolding, building, and testing.
- DO Use Angular's built-in directives and pipes for common tasks.
- DO Use Angular's routing module for navigation and lazy loading of modules.
- DO Follow Angular best practices and conventions for project structure and build.

#### DO NOT

- DO NOT Use unit testing
- DO NOT Use RxJS-based patterns for HTTP handling unless a clear, documented justification is provided.
- DO NOT Use zone.js; prefer zoneless mode.
- DO NOT Use `*ngIf`, `*ngFor`, `ngSwitch` or other structural directives inappropriately; prefer using the new control flow syntax such as `@if`, `@for`, etc.
- DO NOT Use `@Input`, `@Output`, or similar older pattern APIs; instead use signal-based inputs/outputs (e.g. `input.required<string>()`).
- When writing frontend code, reference the project's `.github/instructions/angular.md`.
- When writing styling code, reference the project's `.github/instructions/daisyui.md`.

### 3. Backend: .NET

- Backend services MUST be implemented on .NET 9 using controller APIs.
- Database access MUST use Entity Framework Core.
- Project structure MUST follow .NET best practices and conventions.
- Maintain .http files for API testing and documentation.

#### DO

- DO USE .NET 9 and C# 13 or later for building backend services.
- DO USE Native dependency injection (DI) MUST be used for managing dependencies.
- DO USE Asynchronous programming (async/await) MUST be used for I/O-bound operations.
- DO USE Cancellation tokens and pass them in all async methods to allow for graceful cancellation.
- DO USE LINQ for data manipulation and querying collections.
- DO USE task-based asynchronous patterns for concurrency.
- DO USE Exception handling with try/catch blocks MUST be implemented for robust error management.
- DO write tests that can be run in parallel.

#### DO NOT

- DO NOT write comments
- DO NOT Use synchronous programming for I/O-bound operations.
- DO NOT Use blocking calls in async methods.
- DO NOT Use `Thread.Sleep` for delays; prefer `Task.Delay`.
- DO NOT Use `async void` except for event handlers.

### 4. Testing

- Only write tests for business logic
- DO write integration tests
- DO write end-to-end tests
- DO NOT write unit tests
- DO NOT write performance tests
- DO NOT write load tests
- DO NOT write UI tests
- DO NOT write security tests
- DO NOT write tests for frontend code
- Backend tests MUST use xUnit and Microsoft/.NET native test packages.

## 5. Technology Stack & Constraints

- Frontend: Angular 20 (zoneless), Signals API preferred; avoid RxJS unless justified.
- Angular HTTP: Resource API MUST be used for HTTP/data fetching; RxJS patterns for HTTP are
  prohibited unless justified in the Constitution Check.
- Styling: Tailwind CSS v4, dark-mode-first; use vetted OSS Tailwind libraries as needed.
- Backend: .NET 9.
- Deployment model: Containerised (Docker) services; internal traffic MAY be non-HTTPS.
- Public TLS/HTTP/3: Managed externally (Cloudflare Tunnel or equivalent edge proxy).
- Networking: Minimise outbound calls; design for batched/efficient communication.
- Testing: Backend tests MUST use xUnit and Microsoft/.NET. Do not write frontend tests.
  native test runner/packages.
- Scaffolding: Prefer official CLIs for boilerplate and templates (e.g., `ng generate`,
  `dotnet new`) to ensure up-to-date project structures.
- Performance testing: OPTIONAL unless explicitly required by feature acceptance criteria.

## 6. Environment Configuration & Deployment Constraints

To avoid conflicting deployments and accidental port collisions, all services and tooling
MUST provide separate production and development environment configurations. The project
MUST adopt one of the following patterns (choose per-service and document in the plan):

- Docker compose override files: e.g., `docker-compose.yml`, `docker-compose.dev.yml`,
  `docker-compose.prod.yml`, using different port mappings and service profiles.
- Environment-specific configuration files or environment variables (e.g., `ENV=development`)
  consumed by the service at startup to choose ports and host bindings.
- Container port design: container-internal ports MAY be stable, but host-port mappings
  in development MUST avoid well-known production ports; document recommended development
  ranges and avoid hard-coded host ports when possible.

Plans and CI/CD pipelines MUST ensure that development deployments do not overwrite or
conflict with production deployments. Health checks, metrics endpoints, and admin ports
MUST be namespaced or port-separated between environments. Rationale: prevents accidental
service collisions and simplifies local development while maintaining safe production
deployments.

## 7. Development Workflow & Quality Gates

- Constitution Check: All plans and specs MUST include a short Constitution Check section
  listing how the proposal conforms to each core principle; violations MUST include a
  documented justification and migration or mitigation plan. The plan MUST include how
  production and development environment configurations are separated and how ports are
  selected to avoid conflicts.
- Scaffolding & Code Generation: Prefer official CLI tools (e.g., `ng generate`, `dotnet new`)
  for generating templates/boilerplate to stay current with framework best practices.

## Governance

Amendments to this Constitution follow a lightweight governance process:

1. Propose an amendment as a PR that edits `.specify/memory/constitution.md` and includes a
   clear migration and compliance plan for any affected templates or automation.
2. The PR MUST include a rationale and a version-bump proposal (MAJOR/MINOR/PATCH) with
   reasoning mapped to the versioning rules in this document.
3. Amendments require at least two approvals from project maintainers or owners.
4. For MAJOR changes (principle removal or redefinition) a rollout/migration plan MUST be
   included and a short freeze window MAY be requested.

Versioning policy:

- MAJOR: Backwards-incompatible governance or principle removals/redefinitions.
- MINOR: Addition of new principle(s) or material expansion of guidance.
- PATCH: Wording clarifications, typo fixes, or non-semantic refinements.

**Version**: 1.2.0 | **Ratified**: 2025-09-27 | **Last Amended**: 2025-09-28

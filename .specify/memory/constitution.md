<!--
Sync Impact Report
- Version change: 1.0.0 → 1.1.0
- Modified principles / sections:
  - Added: Environment Configuration (production & development separation)
  - Clarified: Container-first & Edge TLS Delegation (notes about port and host handling)
- Added sections:
  - Environment Configuration & Deployment Constraints
- Removed sections: none
- Templates requiring updates:
  - .specify/templates/plan-template.md ✅ updated (Constitution Check: env config + port conflict gate)
  - .specify/templates/tasks-template.md ✅ updated (setup tasks: env config guidance)
  - .specify/templates/spec-template.md ✅ updated (Technical Context: deployment env requirement)
-->

# Netcoder Constitution

## Core Principles

### I. Lean Runtime & Minimal Network Overhead

The project MUST prioritise a minimal runtime footprint and minimal network overhead. Services
MUST avoid unnecessary outbound network calls, limit payload sizes, and prefer efficient
serialization formats. Designs that introduce chatty protocols or large transfer payloads
MUST provide a documented justification and an explicit mitigation plan (caching, batching,
or compression). Rationale: keeping the runtime lean reduces cost, improves reliability in
container environments, and aligns with the project's expectation of edge TLS termination.

### II. Container-first & Edge TLS Delegation

The project is CONTAINER-FIRST and MAY operate without embedded HTTPS/TLS inside containers.
TLS/HTTP/3 termination and public-facing security MUST be handled by external infrastructure
(e.g., Cloudflare Tunnel, edge proxy). Services MUST validate inbound requests' provenance
and apply mutual authentication or signed headers where appropriate when running in
untrusted environments. Rationale: simplifies service runtime, avoids certificate management
inside containers, and delegates public security to hardened edge services.

### III. Frontend: Angular 20 (Zoneless) & Signals

Frontend applications MUST use Angular 20 in zoneless mode and SHOULD prefer the Signals API
for state management where it provides clear benefits. The project SHOULD avoid adding RxJS
as a primary reactive model; RxJS usage is permitted only when Signals cannot reasonably
express the required behavior (document the justification). The project should use the resource API,
such as the httpResource reactive wrapper around HttpClient that gives you the request status and response
as signals. You can thus use these signals with computed, effect, linkedSignal, or any other reactive API.
Because it's built on top of HttpClient, httpResource supports all the same features, such as interceptors.
Styling MUST use Tailwind CSS v4 with a dark-mode-first approach. The project MAY adopt well-maintained
open-source Tailwind libraries to accelerate UI development, but any dependency MUST be verified for stability
and accessibility. Rationale: reduces runtime overhead, aligns with modern Angular patterns,
and standardises styling across the product.

### IV. Backend: .NET 10 Minimal APIs

Backend services MUST be implemented on .NET 10 using minimal APIs or equivalent lightweight
hosting models. Services MUST be easy to run inside Docker without requiring embedded TLS.
APIs MUST be documented (OpenAPI preferred) and versioned explicitly. Structured logging
(MUST include correlation IDs) and basic metrics exposure (e.g., Prometheus) are REQUIRED
for observability. Rationale: .NET 10 provides long-term support and a predictable runtime
for containerised services while enabling low-overhead hosting.

### V. Simplicity, Testability & Observability

The project values simplicity: designs and implementations MUST favour the simplest solution
that satisfies requirements. Unit and integration tests MUST be present and maintained. The
project DOES NOT REQUIRE dedicated performance test suites as a baseline (performance
testing is OPTIONAL and only required when a feature's acceptance criteria specify
performance goals). Observability (structured logs, key metrics, and error reporting) MUST
be implemented for all services. Rationale: ensures quality and debuggability while avoiding
unnecessary engineering overhead.

## Technology Stack & Constraints

- Frontend: Angular 20 (zoneless), Signals API preferred; avoid RxJS unless justified.
- Styling: Tailwind CSS v4, dark-mode-first; use vetted OSS Tailwind libraries as needed.
- Backend: .NET 10 (minimal APIs preferred).
- Deployment model: Containerised (Docker) services; internal traffic MAY be non-HTTPS.
- Public TLS/HTTP/3: Managed externally (Cloudflare Tunnel or equivalent edge proxy).
- Networking: Minimise outbound calls; design for batched/efficient communication.
- Performance testing: OPTIONAL unless explicitly required by feature acceptance criteria.

## Environment Configuration & Deployment Constraints

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

## Development Workflow & Quality Gates

- Tests: Unit and integration tests MUST be written and run as part of CI. Contract tests
  MUST be created for public service APIs and MUST fail before implementation (TDD
  discipline for contract-driven development).
- Code reviews: All changes MUST go through PRs with at least one approving reviewer.
- Constitution Check: All plans and specs MUST include a short Constitution Check section
  listing how the proposal conforms to each core principle; violations MUST include a
  documented justification and migration or mitigation plan. The plan MUST include how
  production and development environment configurations are separated and how ports are
  selected to avoid conflicts.
- Security: Although internal traffic MAY be non-HTTPS, services MUST validate request
  provenance; secrets MUST be stored using approved secret management solutions.
- Observability: Structured logs, correlation IDs, and basic metrics MUST be included in
  any service that is user-facing or supports critical flows.

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

**Version**: 1.1.0 | **Ratified**: 2025-09-27 | **Last Amended**: 2025-09-27

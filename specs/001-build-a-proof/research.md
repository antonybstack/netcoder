# Phase 0 Research – C# Web Playground POC

Date: 2025-09-27 | Feature: 001-build-a-proof

## Decisions

1. Execution strategy for C#

- Decision: Compile C# 13 source with Roslyn to an in-memory assembly and execute in an isolated worker process with constrained resources.
- Rationale: Full language support, deterministic diagnostics, clear resource boundaries.
- Alternatives considered: C# scripting (Microsoft.CodeAnalysis.CSharp.Scripting) – simpler but different semantics and reduced parity; out-of-process runner with temporary project – slower IO and more moving parts.

1. Sandbox policy

- Decision: Moderate sandbox: temp directory file I/O allowed; no network; no process spawn; enforce time (10s), memory (256 MB), CPU (1 vCPU), output (1 MB) limits.
- Rationale: Balances utility (file writes) and safety for POC; clear error shapes.
- Alternatives: Strict (no file I/O) – reduces usefulness for common simple tasks; Permissive – increases risk.

1. Rate limiting

- Decision: 5 runs/min per IP, burst 10; respond 429 with retry-after 60s.
- Rationale: Prevents abuse for demo; easy to understand.
- Alternatives: Auth‑based quotas – overkill for POC.

1. Editor integration (Angular)

- Decision: Monaco Editor component with C# language mode; run button; output panel with tabs (Output, Errors, System).
- Rationale: Familiar UX; good C# highlighting.
- Alternatives: CodeMirror – acceptable but Monaco has stronger C# ecosystem.

1. Observability

- Decision: Structured logs with correlation ID (X-Request-ID). Log request IDs, duration, outcome; hash(source) only (no raw source in logs). Basic metrics (runs_total, run_duration_ms, run_outcomes).
- Rationale: Useful for debugging without leaking code content.
- Alternatives: No hashing – would leak; omit metrics – reduces visibility.

## Open Questions Resolved

- CPU limit: 1 vCPU time-sliced (container/job level). Kill on breach.
- Output cap: 1 MB combined stdout+stderr; include truncated=true and bytesReturned/bytesTotal.
- Error shapes: { phase: compile|run|timeout|inputNotSupported|oom, message, diagnostics?, exception? }

## References & Notes

- Tailwind CSS v4 + daisyUI 5 for UI, dark-first.
- .NET 10 minimal API; OpenAPI for /api/run.

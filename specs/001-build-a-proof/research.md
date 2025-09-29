# Phase 0 Research: Interactive C# 13 Code Execution PoC

## Decisions

### Execution Engine

- Decision: Use Roslyn C# scripting (Microsoft.CodeAnalysis.CSharp.Scripting) to execute user-submitted top-level C# 13 code.
- Rationale: Fast iteration for PoC, no external process; supports top‑level statements; easy to capture diagnostics.
- Alternatives considered:
  - Compile ephemeral project and run via `dotnet`: Slower, heavier I/O, process management overhead; unnecessary for PoC.
  - Roslyn compile to in‑memory assembly + reflection entry: More boilerplate; scripting API provides simpler surface for snippets.

### Output Capture

- Decision: Redirect `Console.Out` and `Console.Error` to StringWriter during execution; collect up to 1 MB, then truncate with flag.
- Rationale: Matches FR for stdout/stderr and truncation.
- Alternatives: Custom TextWriter stream with ring buffer; deferred to later if needed.

### Timeout

- Decision: Enforce 10‑second execution timeout via `CancellationToken` + `Task.WhenAny` guard around script execution.
- Rationale: Fulfills clarified requirement; simple and reliable.
- Alternatives: AppDomain/process isolation with OS‑level kill; out of scope for PoC.

### Concurrency

- Decision: Allow unlimited concurrent runs per client; defer throttling.
- Rationale: Matches clarification; simplifies PoC.
- Risk: Server resource exhaustion under load; acceptable for internal PoC.

### Security Posture

- Decision: No sandbox; internal trusted usage only.
- Rationale: Per clarification; simplifies implementation.
- Risk: Arbitrary code execution risk; mitigate by restricting environment exposure to internal dev only.

### Language & Versioning

- Decision: Target C# 13 semantics on .NET 9 runtime in current repo; ensure Roslyn package versions support C# 13.
- Rationale: Aligns with feature spec and current backend.

## Implementation Notes (feed into Phase 1)

- Basic model types: CodeSubmission, ExecutionResult, Diagnostic.
- API: POST /api/exec/run accepts code, returns result object.
- Diagnostics: Use Roslyn diagnostics (ID, severity, message, line/column mapping to snippet).
- Observability: Structured logs with requestId, durationMs, outcome.
- Dev environment: bind to <http://localhost:5189> (from launchSettings); no external egress.

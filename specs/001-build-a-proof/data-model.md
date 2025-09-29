# Data Model (Phase 1)

## Entities

### CodeSubmission

- code: string (required, ≤ 1 MB)
- requestId: string (optional client-supplied)
- submittedAt: ISO 8601 timestamp (server-assigned)

### Diagnostic

- id: string (e.g., CS1002)
- severity: enum [Hidden, Info, Warning, Error]
- message: string
- line: integer (1-based)
- column: integer (1-based)

### ExecutionResult

- outcome: enum [Success, CompileError, RuntimeError, Timeout]
- stdout: string (truncated to 1 MB)
- stderr: string (truncated to 1 MB)
- diagnostics: Diagnostic[] (compile/runtime messages)
- durationMs: integer
- truncated: boolean

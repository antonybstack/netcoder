# Phase 1 – Data Model

Date: 2025-09-27 | Feature: 001-build-a-proof

## Entities

### CodeSubmission

- source: string (required, UTF-8)
- language: string (fixed: "csharp")
- options: object (optional)
  - optimize: boolean (default false)
  - target: string (default: "ConsoleApp")
  - requestId: string (optional client-provided; otherwise server generates)

Validation:

- source non-empty; max size 256 KB to protect service

### RunResult

- success: boolean
- phase: enum [compile, run, timeout, inputNotSupported, oom, error]
- stdout: string (≤ 1 MB combined with stderr; may be truncated)
- stderr: string (≤ 1 MB combined with stdout; may be truncated)
- diagnostics: array of Diagnostic
- exception: ExceptionInfo (optional)
- truncated: boolean (true if output limited)
- durationMs: number
- requestId: string
- bytesReturned: number (combined stdout+stderr)
- bytesTotal: number (before truncation)

### Diagnostic

- message: string
- severity: enum [info, warning, error]
- line: number (optional)
- column: number (optional)

### ExceptionInfo

- type: string
- message: string
- stack: string (optional)

## Notes

- Output limits: combined stdout+stderr ≤ 1 MB; when exceeded, truncate and set truncated=true, bytesReturned, bytesTotal.
- Memory cap enforced at 256 MB.
- Execution timeout enforced at 10s.

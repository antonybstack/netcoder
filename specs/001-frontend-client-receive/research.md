# Research: Realtime C# Intellisense & Syntax Feedback

## Decisions

- Editing context: Single-file only.
- MVP scope: Completions, diagnostics, hover, signature help.
- Disconnect behavior: Show status; local syntax-only; auto-retry.
- File size responsiveness: Best-effort; no guaranteed line-count.
- Access: Anonymous allowed; no auth required.

## Rationale

- Single-file reduces complexity and latency; avoids project graph overhead.
- MVP focuses on the highest user value signals for editing.
- Clear offline/latency UX avoids confusion and preserves responsiveness.
- Best-effort file size avoids premature optimization; measure first.
- Anonymous access simplifies onboarding and demo flows.

## Technology Notes

- Backend: .NET 10 SignalR Hub for real-time messages.
- Frontend: `@microsoft/signalr` + Angular 21 Signals (`signal`, `computed`, `effect`), `resource`/`httpResource` for any HTTP.
- Debounce: Client-side debouncing for high-frequency events (typing) to cap message rate.

## Alternatives Considered

- Full project context (multi-file): deferred due to complexity and cost.
- Code actions/navigation in MVP: deferred to reduce scope risk.

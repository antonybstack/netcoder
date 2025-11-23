# Tasks: Realtime C# Intellisense & Syntax Feedback in Editor

**Input**: Design documents from `/specs/001-frontend-client-receive/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Phase 3.1: Setup

- [ ] T001 Backend: Ensure SignalR hub endpoint mapping exists in `backend/CodeApi/Program.cs` and hosting config supports WebSockets.
- [ ] T002 Backend: Add DTO namespace folder `backend/CodeApi/Models/Intellisense/` for feature models.
- [ ] T003 Frontend: Add realtime client dependency `@microsoft/signalr` to `frontend/package.json` and lockfile; verify install.
- [ ] T004 [P] Configure repository CI to run `dotnet build`/`dotnet test` for backend and `ng build` for frontend on PRs and `main`.
- [ ] T005 Environment: Document dev/prod ports and host bindings in `specs/001-frontend-client-receive/quickstart.md` (and repo README if needed) to avoid collisions.

Notes

- Use official CLIs for scaffolding (e.g., `dotnet new`, `ng generate`).

## Phase 3.2: Tests First (TDD)

- [ ] T100 [P] Contract test for SignalR hub messages based on `contracts/signalr-contracts.md` → create `backend/CodeApi.Tests/Contract/Intellisense_SignalR_Contracts.cs`.
- [ ] T101 [P] Integration test: completions appear for `Console.Wri` → `backend/CodeApi.Tests/Integration/Completions_Appears_For_Console.cs`.
- [ ] T102 [P] Integration test: diagnostics shown on syntax error → `backend/CodeApi.Tests/Integration/Diagnostics_On_SyntaxError.cs`.
- [ ] T103 [P] Integration test: hover info shows symbol details → `backend/CodeApi.Tests/Integration/Hover_Shows_Type_Info.cs`.
- [ ] T104 [P] Integration test: signature help updates with argument index → `backend/CodeApi.Tests/Integration/SignatureHelp_Argument_Navigation.cs`.
- [ ] T105 [P] Integration test: disconnect behavior (status shown, local syntax-only, auto-retry) → `backend/CodeApi.Tests/Integration/Disconnects_Degrade_Gracefully.cs`.

Scaffold

- Create common SignalR test fixture to host the app and connect a client (`WebApplicationFactory` + `HubConnection`).

## Phase 3.3: Core Implementation

Models (backend)

- [ ] T200 [P] Add `DocumentRef` and `TextState` models → `backend/CodeApi/Models/Intellisense/DocumentRef.cs`, `TextState.cs`.
- [ ] T201 [P] Add `CompletionItem` model → `backend/CodeApi/Models/Intellisense/CompletionItem.cs`.
- [ ] T202 [P] Add `HoverInfo` model → `backend/CodeApi/Models/Intellisense/HoverInfo.cs`.
- [ ] T203 [P] Add `SignatureHelp` model → `backend/CodeApi/Models/Intellisense/SignatureHelp.cs`.
- [ ] T204 [P] Align/extend `Diagnostic` model for editor (if needed) → `backend/CodeApi/Models/Intellisense/Diagnostic.cs`.
- [ ] T205 [P] Add `EditorSession` model → `backend/CodeApi/Models/Intellisense/EditorSession.cs`.

Services & Hub (backend)

- [ ] T210 Define `ICodeIntellisenseService` and implement `CodeIntellisenseService` → `backend/CodeApi/Services/ICodeIntellisenseService.cs`, `CodeIntellisenseService.cs`.
- [ ] T211 Implement SignalR hub `IntellisenseHub` with methods: `requestCompletions`, `requestDiagnostics`, `requestHover`, `requestSignatureHelp` → `backend/CodeApi/Hubs/IntellisenseHub.cs` and map in `Program.cs`.
- [ ] T212 Add structured logging and correlation IDs to hub/service; debounce high-frequency requests (typing).

Frontend

- [ ] T220 [P] Create TS types for models → `frontend/src/app/types/intellisense.ts` (CompletionItem, Diagnostic, HoverInfo, SignatureHelp, DocumentRef, TextState, EditorSession).
- [ ] T221 Create SignalR client service using Angular signals → `frontend/src/app/services/intellisense.service.ts` (connect, status signal, request APIs).
- [ ] T222 Integrate service into code editor component → `frontend/src/app/components/code-page/` (wire up completions, diagnostics, hover, signature help with signals/resource/httpResource patterns).

Validation & Error Handling

- [ ] T230 Validate payloads and guard single-file context on server; return user-friendly errors.
- [ ] T231 Implement disconnect UX: status indicator, local syntax-only mode, auto-retry.

## Phase 3.4: Integration

- [ ] T300 Configure CORS for SignalR and dev host origins in `backend/CodeApi/Program.cs`.
- [ ] T301 Confirm anonymous access: ensure hub permits unauthenticated connections; disable auth gating for MVP.
- [ ] T302 Request/response logging with minimal payload sampling; add metrics for connections and latency.

## Phase 3.5: Polish

- [ ] T400 [P] Update `specs/001-frontend-client-receive/contracts/signalr-contracts.md` with any deltas.
- [ ] T401 [P] Update quickstart with final ports and run instructions; verify manual flow.
- [ ] T402 [P] Remove duplication, run formatters, ensure always‑green.

## Dependencies

- Setup (T001–T005) before Tests (T100–T105).
- Tests (T100–T105) before Core (T200–T231).
- Backend models (T200–T205) unblock backend services/hub (T210–T212).
- Frontend types (T220) unblock frontend service (T221) and component integration (T222).
- Integration (T300–T302) after core is functional.
- Polish (T400–T402) last.

## Parallel Execution Examples

```text
# In separate terminals (fish):
# Run contract + integration tests scaffolds in parallel (independent files)
# Terminal 1:
pushd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test; popd
# Terminal 2:
pushd /Users/antbly/dev/netcoder/frontend; ng build; popd

# Implement backend models in parallel (different files):
# T200–T205 can be split among contributors.

# Frontend parallel work:
# T220 (types) and T221 (service) can proceed in parallel with backend tests running.
```

## Task Agent Command Hints

- Install frontend dependency (fish): `pushd /Users/antbly/dev/netcoder/frontend; npm i @microsoft/signalr; popd`
- Start backend dev server (fish): `pushd /Users/antbly/dev/netcoder/backend/CodeApi; dotnet run; popd`
- Start frontend dev server (fish): `pushd /Users/antbly/dev/netcoder/frontend; npm start; popd`
- Build and test backend (fish): `pushd /Users/antbly/dev/netcoder/backend/CodeApi.Tests; dotnet test; popd`

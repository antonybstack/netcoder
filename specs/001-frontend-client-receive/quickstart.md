# Quickstart: Realtime C# Intellisense (MVP)

## Prerequisites

- .NET 10 SDK installed
- Node 20+ and pnpm/npm installed

## Environment Ports & Hosts

- **Frontend dev**: `http://localhost:4200`
- **Backend dev**: `http://localhost:5189`
- **Backend production**: bind to `http://0.0.0.0:8080` (override via `ASPNETCORE_URLS`)
- **SignalR hub**: `/hubs/intellisense` (shared in both environments)

Keep dev servers on localhost to avoid collisions with CI (which uses containerized ports 8080/4173).

## Backend (SignalR)

- From `backend/CodeApi`:
  - Run: `dotnet build && ASPNETCORE_URLS=http://localhost:5189 dotnet run`
  - Confirms: app starts and exposes SignalR hub `/hubs/intellisense`

## Frontend (Angular 21)

- From `frontend`:
  - Install deps (already installed): `npm i`
  - Run dev server: `npm start -- --port 4200 --host localhost`

## Verify

- Open the app’s code editor page.
- Type `Console.Wri` and expect completions including `Console.WriteLine` in the Intellisense panel.
- Introduce a syntax error and pause; expect diagnostics to list the error with severity.
- Move the caret over `Console.WriteLine` to view hover info and signature help.
- Disconnect network temporarily; expect the status badge to flip to `reconnecting`, local-only mode to enable, and automatic reconnection with the badge returning to `connected`.

## Always-Green Checks

- Backend: `dotnet build && dotnet test`
- Frontend: `ng build` (if applicable)

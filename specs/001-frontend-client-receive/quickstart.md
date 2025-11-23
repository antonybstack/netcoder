# Quickstart: Realtime C# Intellisense (MVP)

## Prerequisites

- .NET 10 SDK installed
- Node 20+ and pnpm/npm installed

## Backend (SignalR)

- From `backend/CodeApi`:
  - Run: `dotnet build && dotnet run`
  - Confirms: app starts and exposes SignalR hub `/hubs/intellisense`

## Frontend (Angular 21)

- From `frontend`:
  - Install deps (already installed): `npm i`
  - Run dev server: `npm start` or `ng serve`

## Verify

- Open the app’s code editor page.
- Type `Console.Wri` and expect completions including `Console.WriteLine`.
- Introduce a syntax error and pause; expect an inline diagnostic.
- Type inside a method call; expect signature help.
- Disconnect network temporarily; expect status indicator, local syntax-only, and automatic reconnection.

## Always-Green Checks

- Backend: `dotnet build && dotnet test`
- Frontend: `ng build` (if applicable)

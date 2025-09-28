# Quickstart – C# Web Playground POC

Date: 2025-09-27 | Feature: 001-build-a-proof

## Prerequisites

- Node 20+ and npm or pnpm
- .NET 10 SDK
- Docker (optional, for container run)

## Dev environment

- Frontend: <http://localhost:4200>
- Backend: <http://localhost:5080>

## Setup

1. Frontend (Angular app)
   - cd app
   - Install dependencies
   - Ensure Tailwind v4 and daisyUI 5 are configured in `src/styles.css`
   - Run dev server
2. Backend (.NET minimal API)
   - cd backend/src/Api (after created by tasks)
   - Run dev server on port 5080

## Run commands (reference)

- Frontend: `npm start` (or `ng serve --port 4200`)
- Backend: `dotnet run --urls http://localhost:5080`

## Test the API

```bash
curl -sS http://localhost:5080/api/run \
  -H 'Content-Type: application/json' \
  -H 'X-Request-ID: demo-1' \
  -d '{
    "source": "Console.WriteLine(\"Hello, world!\");",
    "language": "csharp"
  }' | jq
```

Expected (example):

```json
{
  "success": true,
  "phase": "run",
  "stdout": "Hello, world!\n",
  "stderr": "",
  "truncated": false,
  "durationMs": 200,
  "requestId": "demo-1"
}
```

## Notes

- Output > 1 MB will be truncated and flagged.
- Reading stdin returns standardized "input not supported" error.
- Memory over 256 MB or execution > 10s terminates with a clear error.

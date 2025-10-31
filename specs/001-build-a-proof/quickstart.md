# Quickstart (Phase 1)

## Run backend (development)

- Prereq: .NET SDK installed.
- From repo root:
  - cd backend/CodeApi
  - dotnet run --launch-profile http
  - Service listens on <http://localhost:5189>

## Test with curl

Hello, world:

```bash
curl -sS -X POST \
  -H 'Content-Type: application/json' \
  -d '{"code":"Console.WriteLine(\"Hello, world!\");"}' \
  http://localhost:5189/api/exec/run | jq .
```

Expected outcome.stdout contains:

```text
Hello, world!
```

## Error example

```bash
curl -sS -X POST \
  -H 'Content-Type: application/json' \
  -d '{"code":"Console.WriteLine(\"OOPS\")"}' \
  http://localhost:5189/api/exec/run | jq .
```

Expected outcome: outcome = CompileError with diagnostics.

## Timeout example

```bash
curl -sS -X POST \
  -H 'Content-Type: application/json' \
  -d '{"code":"while(true) { }"}' \
  http://localhost:5189/api/exec/run | jq .
```

Expected outcome: outcome = Timeout and durationMs around 10000.

## Large output truncation

```bash
curl -sS -X POST \
  -H 'Content-Type: application/json' \
  -d '{"code":"Console.Write(new string(\'a\', 1100000));"}' \
  http://localhost:5189/api/exec/run | jq .
```

Expected outcome: truncated = true and stdout length ≤ 1 MB.

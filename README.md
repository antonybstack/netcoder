# netcoder

Internal PoC repository. Do not expose endpoints to untrusted users. No sandboxing is provided; executing submitted code is unsafe outside trusted environments.

## Dev Ports

- Frontend: `http://localhost:4200`
- Backend API & SignalR: `http://localhost:5189` (override with `ASPNETCORE_URLS`)
- Production containers: bind backend to `0.0.0.0:8080`

See `specs/001-frontend-client-receive/quickstart.md` for end-to-end run instructions.

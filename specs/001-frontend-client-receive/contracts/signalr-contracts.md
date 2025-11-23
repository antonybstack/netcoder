# Contracts: SignalR Messages for Intellisense

## Channels / Methods

- Hub: `/hubs/intellisense`
- Methods (client → server):
  - `requestCompletions(payload)`
  - `requestDiagnostics(payload)`
  - `requestHover(payload)`
  - `requestSignatureHelp(payload)`
- Methods (server → client):
  - `completions(result)`
  - `diagnosticsUpdated(result)`
  - `hoverInfo(result)`
  - `signatureHelp(result)`
  - `statusChanged(status)`

## Schemas (informal)

### Common

- `DocumentRef`: `{ sessionId: string, languageVersion: 'C#14' }`
- `TextState`: `{ content: string, cursorOffset: number }`

### Requests (client → server)

- `requestCompletions`: `{ doc: DocumentRef, text: TextState }`
- `requestDiagnostics`: `{ doc: DocumentRef, text: TextState }`
- `requestHover`: `{ doc: DocumentRef, position: number }`
- `requestSignatureHelp`: `{ doc: DocumentRef, position: number }`

### Responses (server → client)

- `completions`: `{ items: CompletionItem[] }`
- `diagnosticsUpdated`: `{ diagnostics: Diagnostic[] }`
- `hoverInfo`: `HoverInfo`
- `signatureHelp`: `SignatureHelp`
- `statusChanged`: `{ status: 'connected' | 'reconnecting' | 'disconnected' }`

(Types `CompletionItem`, `Diagnostic`, `HoverInfo`, `SignatureHelp` align with data-model.md)

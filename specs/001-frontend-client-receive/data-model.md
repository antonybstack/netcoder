# Data Model: Realtime C# Intellisense

## Entities

### CodeDocument

- id: string (session-unique)
- content: string
- cursorOffset: number
- selections: Array<{ start: number; end: number }>
- languageVersion: 'C#14'

### Diagnostic

- code: string
- message: string
- severity: 'error' | 'warning' | 'info'
- range: { start: number; end: number }

### CompletionItem

- label: string
- kind: 'method' | 'property' | 'class' | 'keyword' | 'snippet' | string
- insertText: string
- detail?: string
- documentation?: string
- sortText?: string

### HoverInfo

- contents: string
- range?: { start: number; end: number }

### SignatureHelp

- signatures: Array<{ label: string; parameters: Array<{ label: string }> }>
- activeSignature: number
- activeParameter: number

### EditorSession

- sessionId: string
- connectionStatus: 'connected' | 'reconnecting' | 'disconnected'
- latencyMsP95?: number
- features: { completions: boolean; diagnostics: boolean; hover: boolean; signatureHelp: boolean }

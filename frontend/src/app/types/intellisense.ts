export type IntellisenseStatusCode = 'connected' | 'reconnecting' | 'disconnected' | 'error';

export interface DocumentRef {
  sessionId: string;
  languageVersion: 'C#14';
}

export interface TextState {
  content: string;
  cursorOffset: number;
}

export interface CompletionItem {
  label: string;
  insertText: string;
  kind: string;
  detail?: string;
  documentation?: string;
  sortText?: string;
}

export interface DiagnosticRange {
  start: number;
  end: number;
}

export interface Diagnostic {
  code: string;
  message: string;
  severity: 'Info' | 'Warning' | 'Error';
  range: DiagnosticRange;
}

export interface HoverInfo {
  contents: string;
  range?: DiagnosticRange;
}

export interface SignatureParameter {
  label: string;
  documentation?: string;
}

export interface SignatureDescription {
  label: string;
  parameters: SignatureParameter[];
}

export interface SignatureHelp {
  signatures: SignatureDescription[];
  activeSignature: number;
  activeParameter: number;
}

export interface IntellisenseStatus {
  status: IntellisenseStatusCode;
  localOnly: boolean;
  message?: string;
}

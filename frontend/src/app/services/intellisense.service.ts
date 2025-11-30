import { Injectable, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import {
  CompletionItem,
  Diagnostic,
  DocumentRef,
  HoverInfo,
  IntellisenseStatus,
  SignatureHelp,
  TextState,
} from '../types/intellisense';

const DEFAULT_STATUS: IntellisenseStatus = {
  status: 'disconnected',
  localOnly: false,
};

@Injectable({ providedIn: 'root' })
export class IntellisenseService {
  private connection?: HubConnection;
  private negotiated = false;
  private readonly document: DocumentRef = {
    sessionId:
      typeof crypto?.randomUUID === 'function'
        ? crypto.randomUUID()
        : Math.random().toString(36).slice(2),
    languageVersion: 'C#14',
  };
  private textState: TextState = { content: '', cursorOffset: 0 };

  readonly status = signal<IntellisenseStatus>({ ...DEFAULT_STATUS });
  readonly completions = signal<CompletionItem[]>([]);
  readonly diagnostics = signal<Diagnostic[]>([]);
  readonly hoverInfo = signal<HoverInfo | null>(null);
  readonly signatureHelp = signal<SignatureHelp | null>(null);

  async connect(): Promise<void> {
    await this.ensureConnection();
  }

  async updateText(content: string, cursorOffset: number): Promise<void> {
    this.textState = { content, cursorOffset };
    await this.ensureConnection();
    if (!this.connection) {
      return;
    }

    const envelope = { doc: this.document, text: this.textState };
    await Promise.all([
      this.connection.send('requestCompletions', envelope),
      this.connection.send('requestDiagnostics', envelope),
    ]);
  }

  async requestHover(position: number): Promise<void> {
    await this.ensureConnection();
    await this.connection?.send('requestHover', { doc: this.document, position });
  }

  async requestSignatureHelp(position: number): Promise<void> {
    await this.ensureConnection();
    await this.connection?.send('requestSignatureHelp', { doc: this.document, position });
  }

  private async ensureConnection(): Promise<void> {
    if (this.connection && this.connection.state === HubConnectionState.Connected) {
      return;
    }

    if (!this.connection) {
      this.connection = new HubConnectionBuilder()
        .withUrl('/hubs/intellisense')
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      this.registerHandlers(this.connection);
    }

    if (!this.negotiated) {
      this.negotiated = true;
      const startPromise = this.connection
        .start()
        .then(() => this.status.set({ status: 'connected', localOnly: false }))
        .catch((err) => {
          console.error('Failed to start SignalR connection', err);
          this.negotiated = false;
          this.status.set({
            status: 'error',
            localOnly: true,
            message: 'Unable to connect to realtime intellisense',
          });
        });
      await startPromise;
    }
  }

  private registerHandlers(connection: HubConnection) {
    connection.on('completions', (payload: { items?: CompletionItem[] }) => {
      this.completions.set(payload?.items ?? []);
    });

    connection.on('diagnosticsUpdated', (payload: { diagnostics?: Diagnostic[] }) => {
      this.diagnostics.set(payload?.diagnostics ?? []);
    });

    connection.on('hoverInfo', (payload: HoverInfo) => {
      this.hoverInfo.set(payload);
    });

    connection.on('signatureHelp', (payload: SignatureHelp) => {
      this.signatureHelp.set(payload);
    });

    connection.on('statusChanged', (payload: IntellisenseStatus) => {
      this.status.set({
        status: payload.status,
        localOnly: payload.localOnly ?? false,
        message: payload.message,
      });
    });

    connection.onclose(() => {
      this.negotiated = false;
      this.status.set({ status: 'disconnected', localOnly: true, message: 'Connection closed' });
    });
  }
}

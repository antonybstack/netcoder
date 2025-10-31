export * from './api.service';

export interface CodeSubmission {
  code: string;
  requestId?: string;
}

export type Severity = 'Hidden' | 'Info' | 'Warning' | 'Error';
export interface Diagnostic {
  id?: string;
  severity: Severity;
  message?: string;
  line: number;
  column: number;
}

export type Outcome = 'Success' | 'CompileError' | 'RuntimeError' | 'Timeout';
export interface ExecutionResult {
  outcome: Outcome;
  stdout: string;
  stderr: string;
  diagnostics: Diagnostic[];
  durationMs: number;
  truncated: boolean;
}

export function createExecApi(baseUrl = '') {
  const run = async (payload: CodeSubmission): Promise<ExecutionResult> => {
    const res = await fetch(`${baseUrl}/api/exec/run`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json() as Promise<ExecutionResult>;
  };
  return { run };
}

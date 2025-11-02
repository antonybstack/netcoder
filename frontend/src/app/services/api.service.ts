import { httpResource } from '@angular/common/http';
import { EffectRef, effect, inject, Injector, runInInjectionContext } from '@angular/core';

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
  const injector = inject(Injector);

  const run = (payload: CodeSubmission): Promise<ExecutionResult> =>
    new Promise<ExecutionResult>((resolve, reject) => {
      let watcher: EffectRef;
      const resource = httpResource<ExecutionResult | undefined>(
        () => ({
          url: `${baseUrl}/api/exec/run`,
          method: 'POST',
          body: { ...payload },
          headers: {
            'Content-Type': 'application/json',
          },
        }),
        { injector }
      );

      runInInjectionContext(injector, () => {
        watcher = effect(() => {
          const status = resource.status();
          if (status === 'resolved' && resource.hasValue()) {
            const value = resource.value() as ExecutionResult;
            watcher.destroy();
            resource.destroy();
            resolve(value);
          } else if (status === 'error') {
            const error = resource.error();
            watcher.destroy();
            resource.destroy();
            reject(error ?? new Error('Execution request failed'));
          }
        });
      });
    });

  return { run };
}

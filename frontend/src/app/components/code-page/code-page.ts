import { Component, signal } from '@angular/core';
import { createExecApi, ExecutionResult } from '../../services/api.service';

@Component({
  selector: 'app-code-page',
  imports: [],
  templateUrl: './code-page.html',
  styleUrl: './code-page.css',
})
export class CodePage {
  code = signal('Console.WriteLine("Hello, world!");');
  results = signal<ExecutionResult[]>([]);
  inflight = signal(0);
  api = createExecApi('');

  async run() {
    const text = this.code();
    this.inflight.set(this.inflight() + 1);
    try {
      const res = await this.api.run({ code: text });
      this.results.update((r) => [res, ...r]);
    } finally {
      this.inflight.set(this.inflight() - 1);
    }
  }

  clear() {
    this.results.set([]);
  }
}

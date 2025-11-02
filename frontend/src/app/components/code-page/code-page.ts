import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, signal } from '@angular/core';
import { createExecApi, ExecutionResult } from '../../services/api.service';
import 'monaco-editor/esm/vs/basic-languages/csharp/csharp.contribution';

type MonacoModule = typeof import('monaco-editor/esm/vs/editor/editor.api');
type MonacoEditor = import('monaco-editor/esm/vs/editor/editor.api').editor.IStandaloneCodeEditor;

let monacoLoader: Promise<MonacoModule> | null = null;

const monacoEnvironment = globalThis as typeof globalThis & {
  MonacoEnvironment?: {
    getWorker(_: string, label: string): Worker;
  };
};

const workerUrls = {
  editor: new URL('monaco-editor/esm/vs/editor/editor.worker.js', import.meta.url),
  json: new URL('monaco-editor/esm/vs/language/json/json.worker.js', import.meta.url),
  css: new URL('monaco-editor/esm/vs/language/css/css.worker.js', import.meta.url),
  html: new URL('monaco-editor/esm/vs/language/html/html.worker.js', import.meta.url),
  typescript: new URL('monaco-editor/esm/vs/language/typescript/ts.worker.js', import.meta.url),
} as const;

function createWorker(url: URL): Worker {
  return new Worker(url, { type: 'module' });
}

if (!monacoEnvironment.MonacoEnvironment) {
  monacoEnvironment.MonacoEnvironment = {
    getWorker: (_: string, label: string) => {
      switch (label) {
        case 'json':
          return createWorker(workerUrls.json);
        case 'css':
        case 'scss':
        case 'less':
          return createWorker(workerUrls.css);
        case 'html':
        case 'handlebars':
        case 'razor':
          return createWorker(workerUrls.html);
        case 'typescript':
        case 'javascript':
          return createWorker(workerUrls.typescript);
        default:
          return createWorker(workerUrls.editor);
      }
    },
  };
}

function loadMonaco(): Promise<MonacoModule> {
  if (!monacoLoader) {
    monacoLoader = import('monaco-editor/esm/vs/editor/editor.api');
  }

  return monacoLoader;
}

@Component({
  selector: 'app-code-page',
  imports: [],
  templateUrl: './code-page.html',
  styleUrl: './code-page.css',
})
export class CodePage implements AfterViewInit, OnDestroy {
  code = signal('Console.WriteLine("Hello, world!");');
  results = signal<ExecutionResult[]>([]);
  inflight = signal(0);
  api = createExecApi('');
  private editor: MonacoEditor | null = null;

  @ViewChild('editor', { static: true }) private editorElement?: ElementRef<HTMLDivElement>;

  async ngAfterViewInit() {
    if (!this.editorElement) {
      return;
    }

    const monaco = await loadMonaco();
    this.editor = monaco.editor.create(this.editorElement.nativeElement, {
      value: this.code(),
      language: 'csharp',
      theme: 'vs-dark',
      automaticLayout: true,
      minimap: { enabled: false },
      fontSize: 14,
    });

    this.editor.onDidChangeModelContent(() => {
      if (!this.editor) {
        return;
      }

      this.code.set(this.editor.getValue());
    });
  }

  ngOnDestroy(): void {
    this.editor?.dispose();
    this.editor = null;
  }

  async run() {
    const text = this.editor?.getValue() ?? this.code();
    this.code.set(text);
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

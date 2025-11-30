using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using CodeApi.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Diagnostic = Microsoft.CodeAnalysis.Diagnostic;
using ModelDiagnostic = CodeApi.Models.Diagnostic;

namespace CodeApi.Services;

public interface ICodeExecutionService
{
    Task<ExecutionResult> ExecuteAsync(string code, CancellationToken ct);
}

public class CodeExecutionService : ICodeExecutionService
{
    private const int OutputLimit = 1_048_576;
    private const int TimeoutMs = 10_000;

    private static readonly SemaphoreSlim Gate = new(1, 1);

    private static readonly string[] DefaultImports =
    {
        "System",
        "System.Collections.Generic",
        "System.IO",
        "System.Linq",
        "System.Text",
        "System.Threading",
        "System.Threading.Tasks"
    };

    private static readonly Lazy<IReadOnlyList<MetadataReference>> MetadataReferences = new(() =>
    {
        string trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;
        return trusted
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    });

    private static readonly ScriptOptions ScriptOptions = Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
        .WithReferences(MetadataReferences.Value)
        .WithImports(DefaultImports);

    public async Task<ExecutionResult> ExecuteAsync(string code, CancellationToken ct)
    {
        await Gate.WaitAsync(ct).ConfigureAwait(false);

        BoundedStringWriter stdoutWriter = new BoundedStringWriter(OutputLimit);
        BoundedStringWriter stderrWriter = new BoundedStringWriter(OutputLimit);
        Stopwatch stopwatch = Stopwatch.StartNew();

        TextWriter originalOut = Console.Out;
        TextWriter originalErr = Console.Error;
        bool consoleRedirected = false;

        try
        {
            Script<object>? script = CSharpScript.Create(code, ScriptOptions.WithFilePath("UserSubmission.csx"));
            ImmutableArray<Diagnostic> diagnostics = script.Compile();
            List<Diagnostic> errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0)
            {
                return BuildResult(Outcome.CompileError, stdoutWriter, stderrWriter, stopwatch.ElapsedMilliseconds, false, errors.Select(MapDiagnostic));
            }

            Console.SetOut(stdoutWriter);
            Console.SetError(stderrWriter);
            consoleRedirected = true;

            using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            Task<ScriptState<object>>? runTask = script.RunAsync(cancellationToken: timeoutCts.Token);
            Task timeoutTask = Task.Delay(TimeoutMs, ct);
            Task completedTask = await Task.WhenAny(runTask, timeoutTask).ConfigureAwait(false);
            if (completedTask != runTask)
            {
                if (ct.IsCancellationRequested)
                {
                    timeoutCts.Cancel();
                    throw new OperationCanceledException(ct);
                }

                timeoutCts.Cancel();
                return BuildResult(Outcome.Timeout, stdoutWriter, stderrWriter, stopwatch.ElapsedMilliseconds, true, Enumerable.Empty<ModelDiagnostic>());
            }

            try
            {
                await runTask.ConfigureAwait(false);
                return BuildResult(Outcome.Success, stdoutWriter, stderrWriter, stopwatch.ElapsedMilliseconds, false, Enumerable.Empty<ModelDiagnostic>());
            }
            catch (CompilationErrorException ex)
            {
                return BuildResult(Outcome.CompileError, stdoutWriter, stderrWriter, stopwatch.ElapsedMilliseconds, false, ex.Diagnostics.Select(MapDiagnostic));
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                return BuildResult(Outcome.Timeout, stdoutWriter, stderrWriter, stopwatch.ElapsedMilliseconds, true, Enumerable.Empty<ModelDiagnostic>());
            }
            catch (Exception ex)
            {
                stderrWriter.WriteLine(ex.ToString());
                return BuildResult(Outcome.RuntimeError, stdoutWriter, stderrWriter, stopwatch.ElapsedMilliseconds, false, Enumerable.Empty<ModelDiagnostic>());
            }
        }
        finally
        {
            if (consoleRedirected)
            {
                Console.SetOut(originalOut);
                Console.SetError(originalErr);
            }

            Gate.Release();
        }
    }

    private static ModelDiagnostic MapDiagnostic(Microsoft.CodeAnalysis.Diagnostic diagnostic)
    {
        Location location = diagnostic.Location;
        int line = 0;
        int column = 0;
        if (location.IsInSource)
        {
            FileLinePositionSpan span = location.GetLineSpan();
            line = span.StartLinePosition.Line + 1;
            column = span.StartLinePosition.Character + 1;
        }

        return new ModelDiagnostic
        {
            Id = diagnostic.Id,
            Severity = diagnostic.Severity switch
            {
                DiagnosticSeverity.Hidden => Severity.Hidden,
                DiagnosticSeverity.Info => Severity.Info,
                DiagnosticSeverity.Warning => Severity.Warning,
                _ => Severity.Error
            },
            Message = diagnostic.GetMessage(),
            Line = line,
            Column = column
        };
    }

    private static ExecutionResult BuildResult(
        Outcome outcome,
        BoundedStringWriter stdoutWriter,
        BoundedStringWriter stderrWriter,
        long elapsedMs,
        bool timeout,
        IEnumerable<ModelDiagnostic> diagnostics)
    {
        return new ExecutionResult
        {
            Outcome = outcome,
            Stdout = stdoutWriter.ToString(),
            Stderr = stderrWriter.ToString(),
            Diagnostics = diagnostics.ToList(),
            DurationMs = (int)Math.Min(int.MaxValue, elapsedMs),
            Truncated = timeout || stdoutWriter.IsTruncated || stderrWriter.IsTruncated
        };
    }

    private sealed class BoundedStringWriter : TextWriter
    {
        private readonly int _limit;
        private readonly StringBuilder _buffer = new();

        public bool IsTruncated { get; private set; }

        public override Encoding Encoding => Encoding.UTF8;

        public BoundedStringWriter(int limit)
        {
            _limit = limit;
        }

        public override void Write(char value)
        {
            if (_buffer.Length < _limit)
            {
                _buffer.Append(value);
            }
            else
            {
                IsTruncated = true;
            }
        }

        public override void Write(char[]? buffer, int index, int count)
        {
            if (buffer is null)
            {
                return;
            }

            int remaining = Math.Max(0, _limit - _buffer.Length);
            if (remaining <= 0)
            {
                IsTruncated = true;
                return;
            }

            int sliceCount = Math.Min(count, remaining);
            _buffer.Append(buffer, index, sliceCount);
            if (sliceCount < count)
            {
                IsTruncated = true;
            }
        }

        public override void Write(ReadOnlySpan<char> buffer)
        {
            int remaining = Math.Max(0, _limit - _buffer.Length);
            if (remaining <= 0)
            {
                IsTruncated = true;
                return;
            }

            if (buffer.Length <= remaining)
            {
                _buffer.Append(buffer);
            }
            else
            {
                _buffer.Append(buffer[..remaining]);
                IsTruncated = true;
            }
        }

        public override void Write(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            int remaining = Math.Max(0, _limit - _buffer.Length);
            if (remaining <= 0)
            {
                IsTruncated = true;
                return;
            }

            if (value.Length <= remaining)
            {
                _buffer.Append(value);
            }
            else
            {
                _buffer.Append(value.AsSpan(0, remaining));
                IsTruncated = true;
            }
        }

        public override string ToString() => _buffer.ToString();
    }
}

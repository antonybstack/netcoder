using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeApi.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ModelDiagnostic = CodeApi.Models.Diagnostic;

namespace CodeApi.Services;

public class CodeExecutionService : ICodeExecutionService
{
    private const int OutputLimit = 1048576;

    private static readonly SemaphoreSlim Gate = new(1, 1);

    private const int TimeoutMs = 5_000;
    // ref: https://github.com/LVpuhovs/Programmer-s-Challenge/blob/dcab6d79a4962908a8345359b25794d511b5d2d7/Game%20for%20programming/Game.cs#L154
    public async Task<ExecutionResult> ExecuteAsync(string code, CancellationToken ct)
    {
        await Gate.WaitAsync(ct);

        string? tempDirectory = null;
        var stdoutBuilder = new StringBuilder(OutputLimit * 2);
        var stderrBuilder = new StringBuilder(OutputLimit * 2);
        var sw = Stopwatch.StartNew();

        try
        {
            var source = WrapSubmission(code);
            var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
            var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);

            var compilation = CSharpCompilation.Create(
                assemblyName: $"Submission_{Guid.NewGuid():N}",
                syntaxTrees: new[] { syntaxTree },
                references: MetadataReferences.Value,
                options: new CSharpCompilationOptions(OutputKind.ConsoleApplication, optimizationLevel: OptimizationLevel.Release));

            var diagnostics = compilation.GetDiagnostics();
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Count > 0)
            {
                IList<ModelDiagnostic> mapped = errors.Select(MapDiagnostic).ToList();
                return BuildResult(Outcome.CompileError, stdoutBuilder.ToString(), stderrBuilder.ToString(), sw.ElapsedMilliseconds, false, mapped);
            }

            tempDirectory = CreateTempDirectory();
            var assemblyPath = Path.Combine(tempDirectory, "submission.dll");
            var runtimeConfigPath = Path.Combine(tempDirectory, "submission.runtimeconfig.json");

            WriteRuntimeConfig(runtimeConfigPath);

            var emitResult = compilation.Emit(assemblyPath);
            if (!emitResult.Success)
            {
                IList<ModelDiagnostic> mapped = emitResult.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(MapDiagnostic)
                    .ToList();
                return BuildResult(Outcome.CompileError, stdoutBuilder.ToString(), stderrBuilder.ToString(), sw.ElapsedMilliseconds, false, mapped);
            }

            var startInfo = new ProcessStartInfo("dotnet")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = tempDirectory
            };

            startInfo.ArgumentList.Add("exec");
            startInfo.ArgumentList.Add("--runtimeconfig");
            startInfo.ArgumentList.Add(runtimeConfigPath);
            startInfo.ArgumentList.Add(assemblyPath);

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                stderrBuilder.AppendLine("Failed to start execution process.");
                return BuildResult(Outcome.RuntimeError, stdoutBuilder.ToString(), stderrBuilder.ToString(), sw.ElapsedMilliseconds, false, []);
            }

            var readStdoutTask = ReadStreamAsync(process.StandardOutput, stdoutBuilder);
            var readStderrTask = ReadStreamAsync(process.StandardError, stderrBuilder);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(TimeoutMs));

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                await process.WaitForExitAsync();
                await Task.WhenAll(readStdoutTask, readStderrTask);
                return BuildResult(Outcome.Timeout, stdoutBuilder.ToString(), stderrBuilder.ToString(), sw.ElapsedMilliseconds, true, []);
            }

            await Task.WhenAll(readStdoutTask, readStderrTask);

            if (process.ExitCode == 0)
            {
                return BuildResult(Outcome.Success, stdoutBuilder.ToString(), stderrBuilder.ToString(), sw.ElapsedMilliseconds, false, []);
            }

            return BuildResult(Outcome.RuntimeError, stdoutBuilder.ToString(), stderrBuilder.ToString(), sw.ElapsedMilliseconds, false, []);
        }
        finally
        {
            if (tempDirectory is not null)
            {
                TryDeleteDirectory(tempDirectory);
            }

            Gate.Release();
        }
    }

    static ModelDiagnostic MapDiagnostic(Microsoft.CodeAnalysis.Diagnostic d)
    {
        var span = d.Location.GetLineSpan();
        return new ModelDiagnostic
        {
            Id = d.Id,
            Severity = d.Severity switch
            {
                DiagnosticSeverity.Hidden => Severity.Hidden,
                DiagnosticSeverity.Info => Severity.Info,
                DiagnosticSeverity.Warning => Severity.Warning,
                _ => Severity.Error
            },
            Message = d.GetMessage(),
            Line = span.StartLinePosition.Line + 1,
            Column = span.StartLinePosition.Character + 1
        };
    }

    static readonly Lazy<IReadOnlyList<MetadataReference>> MetadataReferences = new(() =>
    {
        var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;
        return trusted
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToArray();
    });

    static void WriteRuntimeConfig(string path)
    {
        var version = RuntimeFrameworkVersion.Value;
        var runtimeConfig = $"{{\n  \"runtimeOptions\": {{\n    \"tfm\": \"net9.0\",\n    \"framework\": {{\n      \"name\": \"Microsoft.NETCore.App\",\n      \"version\": \"{version}\"\n    }}\n  }}\n}}\n";
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, runtimeConfig);
    }

    static async Task ReadStreamAsync(StreamReader reader, StringBuilder builder)
    {
        var buffer = new char[4096];
        while (true)
        {
            var read = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            if (builder.Length < OutputLimit * 2)
            {
                var allowed = Math.Min(read, OutputLimit * 2 - builder.Length);
                builder.Append(buffer, 0, allowed);
            }
        }
    }

    static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // ignored
        }
    }

    static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "codeapi", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // best effort
        }
    }

    static string WrapSubmission(string code)
    {
        var builder = new StringBuilder();
        builder.AppendLine("#nullable enable");
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Threading.Tasks;");
        builder.AppendLine("using System.Linq;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine();
        builder.AppendLine("public static class SubmissionEntry");
        builder.AppendLine("{");
        builder.AppendLine("    public static async Task<int> Main(string[] args)");
        builder.AppendLine("    {");
        builder.AppendLine("        try");
        builder.AppendLine("        {");
        builder.AppendLine("#line 1 \"UserSubmission\"");
        builder.AppendLine(code);
        builder.AppendLine("#line default");
        builder.AppendLine("            return 0;");
        builder.AppendLine("        }");
        builder.AppendLine("        catch (Exception ex)");
        builder.AppendLine("        {");
        builder.AppendLine("            Console.Error.WriteLine(ex);");
        builder.AppendLine("            return 1;");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    static readonly Lazy<string> RuntimeFrameworkVersion = new(() =>
    {
        try
        {
            var assemblyPath = typeof(object).Assembly.Location;
            var fileVersion = FileVersionInfo.GetVersionInfo(assemblyPath).ProductVersion;
            if (!string.IsNullOrWhiteSpace(fileVersion))
            {
                var plusIndex = fileVersion!.IndexOf('+');
                return plusIndex >= 0 ? fileVersion[..plusIndex] : fileVersion;
            }
        }
        catch
        {
            // fallback below
        }

        return Environment.Version.ToString();
    });

    ExecutionResult BuildResult(Outcome outcome, string outStr, string errStr, long elapsedMs, bool timeout, IList<ModelDiagnostic> diags)
    {
        var truncatedOut = outStr.Length >= OutputLimit;
        var truncatedErr = errStr.Length >= OutputLimit;
        var stdout = truncatedOut ? outStr[..OutputLimit] : outStr;
        var stderr = truncatedErr ? errStr[..OutputLimit] : errStr;
        return new ExecutionResult
        {
            Outcome = outcome,
            Stdout = stdout,
            Stderr = stderr,
            Diagnostics = diags.ToList(),
            DurationMs = (int)Math.Min(int.MaxValue, elapsedMs),
            Truncated = timeout || truncatedOut || truncatedErr
        };
    }
}

using System.Collections.Generic;

namespace CodeApi.Models;

public enum Outcome
{
    Success,
    CompileError,
    RuntimeError,
    Timeout
}

public class ExecutionResult
{
    public Outcome Outcome { get; set; }
    public string Stdout { get; set; } = string.Empty;
    public string Stderr { get; set; } = string.Empty;
    public List<Diagnostic> Diagnostics { get; set; } = new();
    public int DurationMs { get; set; }
    public bool Truncated { get; set; }
}

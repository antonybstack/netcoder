namespace CodeApi.Models.Intellisense;

public enum DiagnosticSeverity
{
    Info,
    Warning,
    Error
}

public class Diagnostic
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Info;

    public TextRange Range { get; set; } = new();
}

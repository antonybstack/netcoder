namespace CodeApi.Models;

public enum Severity
{
    Hidden,
    Info,
    Warning,
    Error
}

public class Diagnostic
{
    public string? Id { get; set; }
    public Severity Severity { get; set; }
    public string? Message { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
}

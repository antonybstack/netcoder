namespace CodeApi.Models.Intellisense;

public class HoverInfo
{
    public string Contents { get; set; } = string.Empty;

    public TextRange? Range { get; set; }
}

using Tapper;

namespace CodeApi.Models.Intellisense;

[TranspilationSource]
public class AppCompletionItem
{
    // Parameterless ctor required for System.Text.Json deserialization
    public AppCompletionItem()
    {
    }

    // Preserve existing convenience ctor
    public AppCompletionItem(
        string displayText,
        string kind,
        string? filterText = null,
        string? sortText = null,
        string? insertText = null,
        InsertTextFormat? insertTextFormat = null,
        string? documentation = null)
    {
        DisplayText = displayText ?? throw new ArgumentNullException(nameof(displayText));
        Kind = kind ?? throw new ArgumentException(nameof(kind));
        FilterText = filterText;
        SortText = sortText;
        InsertText = insertText ?? displayText;
        InsertTextFormat = insertTextFormat;
        Documentation = documentation;
    }

    public string DisplayText { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;

    public string? FilterText { get; set; }

    public string? SortText { get; set; }

    public string InsertText { get; set; } = string.Empty;

    public InsertTextFormat? InsertTextFormat { get; set; }

    public string? Documentation { get; set; }

    internal object? AssociatedSymbol { get; set; }

    public override string ToString()
    {
        return DisplayText;
    }
}
namespace CodeApi.Models.Intellisense;

public class AppCompletionItem(
    string displayText,
    string kind,
    string filterText = null,
    string sortText = null,
    string insertText = null,
    InsertTextFormat? insertTextFormat = null,
    string documentation = null)
{
    public string DisplayText { get; } = displayText ?? throw new ArgumentNullException(nameof(displayText));

    public string Kind { get; } = kind ?? throw new ArgumentException(nameof(kind));

    public string FilterText { get; } = filterText;

    public string SortText { get; } = sortText;

    public string InsertText { get; } = insertText ?? displayText;

    public InsertTextFormat? InsertTextFormat { get; } = insertTextFormat;

    public string Documentation { get; set; } = documentation;

    internal object? AssociatedSymbol { get; set; }

    public override string ToString()
    {
        return DisplayText;
    }
}
using CodeApi.Models.Intellisense;

namespace CodeApi.Hubs;

public partial class IntellisenseHub
{
    public record CompletionsResponse(IReadOnlyList<AppCompletionItem> items)
    {
        public override string ToString()
        {
            return $"{{ items = {items} }}";
        }
    }
}
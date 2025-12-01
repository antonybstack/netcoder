using CodeApi.Models.Intellisense;
using Tapper;

namespace CodeApi.Hubs;

[TranspilationSource]
public class CompletionsResponse
{
    /*public CompletionsResponse()
    {
        Items = [];
    }*/

    public CompletionsResponse(IReadOnlyList<AppCompletionItem>? items)
    {
        Items = items?.ToList() ?? [];
    }

    public IReadOnlyList<AppCompletionItem> Items { get; set; }
}
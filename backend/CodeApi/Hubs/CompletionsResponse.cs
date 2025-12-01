using System.Text.Json.Serialization;
using CodeApi.Models.Intellisense;

namespace CodeApi.Hubs;

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
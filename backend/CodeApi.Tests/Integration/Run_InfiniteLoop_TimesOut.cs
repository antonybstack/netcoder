using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Run_InfiniteLoop_TimesOut
{
    [Fact]
    public async Task Should_Timeout()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var payload = new { code = "await Task.Delay(11000);" };
        var response = await client.PostAsJsonAsync("/api/exec/run", payload);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(json);
        Assert.Equal("Timeout", json!["outcome"]!.GetValue<string>());
        Console.WriteLine(json["durationMs"]!.GetValue<int>());
        Assert.InRange(json["durationMs"]!.GetValue<int>(), 3000, 15000);
    }
}

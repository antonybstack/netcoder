using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Run_InfiniteLoop_TimesOut
{
    [Fact]
    public async Task Should_Timeout()
    {
        await using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        var payload = new { code = "await Task.Delay(11000);" };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/exec/run", payload);
        response.EnsureSuccessStatusCode();
        JsonObject? json = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(json);
        Assert.Equal("Timeout", json!["outcome"]!.GetValue<string>());
        Console.WriteLine(json["durationMs"]!.GetValue<int>());
        Assert.InRange(json["durationMs"]!.GetValue<int>(), 9000, 14000);
        Assert.True(json["truncated"]!.GetValue<bool>());
        Assert.Empty(json["diagnostics"]!.AsArray());
    }
}

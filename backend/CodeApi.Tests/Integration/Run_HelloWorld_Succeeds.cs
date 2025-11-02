using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Run_HelloWorld_Succeeds
{
    [Fact]
    public async Task Should_Return_Success_And_Output()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var payload = new { code = "Console.WriteLine(\"Hello, world!\");" };
        var response = await client.PostAsJsonAsync("/api/exec/run", payload);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(json);
        Assert.Equal("Success", json!["outcome"]!.GetValue<string>());
        var stdout = json["stdout"]!.GetValue<string>();
        Assert.Contains("Hello, world!", stdout);
        Assert.Equal(string.Empty, json["stderr"]!.GetValue<string>());
        Assert.Empty(json["diagnostics"]!.AsArray());
        Assert.False(json["truncated"]!.GetValue<bool>());
        Assert.InRange(json["durationMs"]!.GetValue<int>(), 0, 2_000);
    }
}

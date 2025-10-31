using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Run_LargeOutput_Truncated
{
    [Fact]
    public async Task Should_Truncate_Output_At_1MB()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var payload = new { code = "Console.Write(new string('a', 1100000));" };
        var response = await client.PostAsJsonAsync("/api/exec/run", payload);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(json);
        Assert.True(json!["truncated"]!.GetValue<bool>());
        var stdout = json["stdout"]!.GetValue<string>();
        Assert.True(stdout.Length <= 1048576);
    }
}

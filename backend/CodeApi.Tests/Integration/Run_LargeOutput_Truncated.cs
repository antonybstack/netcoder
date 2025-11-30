using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class Run_LargeOutput_Truncated
{
    [Fact]
    public async Task Should_Truncate_Output_At_1MB()
    {
        await using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();
        var payload = new { code = "Console.Write(new string('a', 1100000));" };
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/exec/run", payload);
        response.EnsureSuccessStatusCode();
        JsonObject? json = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(json);
        Assert.Equal("Success", json!["outcome"]!.GetValue<string>());
        Assert.True(json!["truncated"]!.GetValue<bool>());
        string stdout = json["stdout"]!.GetValue<string>();
        Assert.Equal(1_048_576, stdout.Length);
        Assert.Equal(string.Empty, json["stderr"]!.GetValue<string>());
        Assert.Empty(json["diagnostics"]!.AsArray());
    }
}

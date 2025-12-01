using System.Text.Json;
using System.Text.Json.Serialization;
using CodeApi.Hubs;
using CodeApi.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    JsonSerializerOptions jsonOptions = o.JsonSerializerOptions;
    jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    jsonOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    jsonOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddOpenApi();
builder.Services
    .AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PayloadSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddScoped<ICodeExecutionService, CodeExecutionService>();
builder.Services.AddTransient<IRoslynCompletionService, RoslynCompletionService>();


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("dev", policy => policy
            .WithOrigins("http://localhost:4200", "http://localhost:5173", "http://localhost:26200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    });
}

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("dev");
}

app.UseAuthorization();
app.MapControllers();
app.MapHub<IntellisenseHub>("/hubs/intellisense");
app.Run();
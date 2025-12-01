using CodeApi.Models;
using CodeApi.Models.Intellisense;
using CodeApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Text;

namespace CodeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompletionsController : ControllerBase
{
    private const int MaxCodeLength = 1_048_576;

    private readonly IRoslynCompletionService _completionService;
    private readonly ILogger<CompletionsController> _logger;

    public CompletionsController(IRoslynCompletionService completionService, ILogger<CompletionsController> logger)
    {
        _completionService = completionService;
        _logger = logger;
    }

    [HttpPost("")]
    public async Task<ActionResult<IReadOnlyList<AppCompletionItem>>> GetCompletions([FromBody] CompletionRequest? request)
    {
        if (request is null)
        {
            ModelState.AddModelError(nameof(CompletionRequest), "Request payload is required.");
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return Ok(Array.Empty<AppCompletionItem>());
        }

        if (request.Code.Length > MaxCodeLength)
        {
            ModelState.AddModelError(nameof(CompletionRequest.Code),
                $"Code must be \u2264 {MaxCodeLength} characters.");
        }

        if (request.LineNumber < 1)
        {
            ModelState.AddModelError(nameof(CompletionRequest.LineNumber), "Line number must be \u2265 1.");
        }

        if (request.Column < 1)
        {
            ModelState.AddModelError(nameof(CompletionRequest.Column), "Column must be \u2265 1.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            SourceText sourceText = SourceText.From(request.Code);

            if (request.LineNumber > sourceText.Lines.Count || request.LineNumber < 1)
            {
                ModelState.AddModelError(nameof(CompletionRequest.LineNumber), "Invalid line number.");
                return ValidationProblem(ModelState);
            }

            TextLine line = sourceText.Lines[request.LineNumber - 1];
            int intraLineIndex = Math.Clamp(request.Column - 1, 0, line.Span.Length);
            int cursorPosition = Math.Clamp(line.Start + intraLineIndex, 0, sourceText.Length);

            IReadOnlyList<AppCompletionItem> suggestions = await _completionService.GetCompletionsAsync(
                request.Code,
                // TODO: Fix this
                //cursorPosition,
                string.Empty,
                HttpContext.RequestAborted);

            return Ok(suggestions);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute completions");
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to compute completions.");
        }
    }
}
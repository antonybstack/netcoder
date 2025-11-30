using CodeApi.Models;
using CodeApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeApi.Controllers;

[ApiController]
[Route("api/exec")]
public class ExecController : ControllerBase
{
    private const int MaxCodeLength = 1_048_576;

    readonly ICodeExecutionService _service;
    readonly ILogger<ExecController> _logger;

    public ExecController(ICodeExecutionService service, ILogger<ExecController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost("run")]
    public async Task<ActionResult<ExecutionResult>> Run([FromBody] CodeSubmission submission)
    {
        if (submission is null)
        {
            ModelState.AddModelError(nameof(CodeSubmission.Code), "Submission payload is required.");
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(submission.Code))
        {
            ModelState.AddModelError(nameof(CodeSubmission.Code), "Code must not be empty.");
            return ValidationProblem(ModelState);
        }

        if (submission.Code.Length > MaxCodeLength)
        {
            ModelState.AddModelError(nameof(CodeSubmission.Code), $"Code must be \u2264 {MaxCodeLength} characters.");
            return ValidationProblem(ModelState);
        }

        submission.SubmittedAt = DateTime.UtcNow;
        submission.RequestId ??= HttpContext.TraceIdentifier;

        ExecutionResult result = await _service.ExecuteAsync(submission.Code, HttpContext.RequestAborted);
        _logger.LogInformation(
            "Execution completed for {RequestId} with {Outcome} in {DurationMs}ms",
            submission.RequestId,
            result.Outcome,
            result.DurationMs);
        return Ok(result);
    }
}

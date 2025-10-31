using CodeApi.Models;
using CodeApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeApi.Controllers;

[ApiController]
[Route("api/exec")]
public class ExecController : ControllerBase
{
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
        if (submission == null || string.IsNullOrWhiteSpace(submission.Code)) return BadRequest();
        if (submission.Code.Length > 1048576) return BadRequest();
        submission.SubmittedAt = DateTime.UtcNow;
        var result = await _service.ExecuteAsync(submission.Code, HttpContext.RequestAborted);
        _logger.LogInformation("requestId={RequestId} outcome={Outcome} durationMs={Duration}", submission.RequestId, result.Outcome, result.DurationMs);
        return Ok(result);
    }
}

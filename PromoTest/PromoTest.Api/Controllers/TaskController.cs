using Microsoft.AspNetCore.Mvc;
using PromoTest.Api.Domain;
using PromoTest.Api.DTO;
using PromoTest.Api.Services.Interfaces;

namespace PromoTest.Api.Controllers;

[ApiController]
[Route("tasks")]
public sealed class TaskController(ITaskService taskService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { error = "Title is required" });
        }

        if (request.Title.Length > 200)
        {
            return BadRequest(new { error = "Title must not exceed 200 characters" });
        }

        if (request.Priority.HasValue && !Enum.IsDefined(request.Priority.Value))
        {
            return BadRequest(new { error = "Invalid priority" });
        }

        var task = await taskService.CreateAsync(request with { Title = request.Title.Trim() }, cancellationToken);

        return CreatedAtAction(nameof(GetAll), new { id = task.Id }, task);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskItem>>> GetAll(CancellationToken cancellationToken)
    {
        var tasks = await taskService.GetAllAsync(cancellationToken);

        return Ok(tasks);
    }

    [HttpPut("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await taskService.CompleteAsync(id, cancellationToken);

        return result switch
        {
            CompleteTaskResult.Success => Ok(),
            CompleteTaskResult.NotFound => NotFound(),
            CompleteTaskResult.Conflict => Conflict(new { error = "Task is already completed or was modified concurrently." }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await taskService.DeleteAsync(id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
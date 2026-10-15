using Microsoft.EntityFrameworkCore;
using PromoTest.Api.Domain;
using PromoTest.Api.Domain.Common.Enum;
using PromoTest.Api.DTO;
using PromoTest.Api.Persistence;
using PromoTest.Api.Services.Interfaces;
using PromoTest.Api.Services.Messaging;

namespace PromoTest.Api.Services;

public sealed class TaskService(
    EfContext db,
    ITaskEventPublisher eventPublisher,
    ILogger<TaskService> logger) : ITaskService
{
    public async Task<TaskItem> CreateAsync(
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Title = request.Title!,
            Priority = request.Priority ?? Priority.Medium,
            IsCompleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            CompletedAt = null
        };

        db.TaskItems.Add(task);
        await db.SaveChangesAsync(cancellationToken);

        return task;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await db.TaskItems
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CompleteTaskResult> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var task = await db.TaskItems.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);

        if (task is null)
            return CompleteTaskResult.NotFound;

        if (task.IsCompleted)
            return CompleteTaskResult.Conflict;

        task.IsCompleted = true;
        task.CompletedAt = DateTimeOffset.UtcNow;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CompleteTaskResult.Conflict;
        }

        try
        {
            await eventPublisher.EnqueueAsync(
                new TaskCompletedEvent(
                    task.Id,
                    task.Title,
                    task.CompletedAt.Value,
                    task.Priority.ToString()));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to enqueue completion event for task {TaskId}",
                task.Id);
        }

        return CompleteTaskResult.Success;
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var task = await db.TaskItems.FirstOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);

        if (task is null)
            return false;

        db.TaskItems.Remove(task);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
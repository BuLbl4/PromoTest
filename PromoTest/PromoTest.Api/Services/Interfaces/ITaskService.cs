using PromoTest.Api.Domain;
using PromoTest.Api.DTO;

namespace PromoTest.Api.Services.Interfaces;

public interface ITaskService
{
    Task<TaskItem> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken);
    Task<IEnumerable<TaskItem>> GetAllAsync(CancellationToken cancellationToken);
    Task<CompleteTaskResult> CompleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
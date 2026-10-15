using PromoTest.Api.Services.Messaging;

namespace PromoTest.Api.Services.Interfaces;

public interface ITaskEventPublisher
{
    ValueTask EnqueueAsync(TaskCompletedEvent taskItem);
}
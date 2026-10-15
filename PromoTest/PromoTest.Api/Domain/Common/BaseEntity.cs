namespace PromoTest.Api.Domain.Common;

public class BaseEntity
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public DateTimeOffset? CompletedAt { get; set; }
}
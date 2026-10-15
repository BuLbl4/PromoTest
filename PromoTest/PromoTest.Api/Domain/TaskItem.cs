using PromoTest.Api.Domain.Common;
using PromoTest.Api.Domain.Common.Enum;

namespace PromoTest.Api.Domain;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; }
    
    public Priority Priority { get; set; }
}
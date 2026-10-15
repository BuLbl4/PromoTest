using PromoTest.Api.Domain.Common.Enum;

namespace PromoTest.Api.DTO;

public sealed record CreateTaskRequest(string? Title, Priority? Priority);

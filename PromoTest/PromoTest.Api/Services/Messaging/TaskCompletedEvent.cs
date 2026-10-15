namespace PromoTest.Api.Services.Messaging;

public sealed record TaskCompletedEvent(Guid Id, string Title, DateTimeOffset CompletedAt, string Priority);
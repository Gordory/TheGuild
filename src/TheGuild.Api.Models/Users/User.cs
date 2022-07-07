namespace TheGuild.Api.Models.Users;

public record User
{
    public Guid Id { get; init; }

    public string? DisplayedName { get; init; }
}
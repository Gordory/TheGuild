namespace TheGuild.Api.Models.Users.Discord;

public record DiscordBot : User
{
    public ulong DiscordApplicationId { get; init; }

    // todo: (Ильиных Никита Сергеевич/06.07.2022/23:51): Сделать дискорд бота-пользователя API
}
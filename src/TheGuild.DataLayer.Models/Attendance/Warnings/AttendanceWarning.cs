using MongoDB.Bson.Serialization.Attributes;
using TheGuild.DataLayer.Models.Access;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Entities;

namespace TheGuild.DataLayer.Models.Attendance.Warnings;

[MongoCollection("AttendanceWarnings")]
public record AttendanceWarning : IIdentifiedEntity<Guid>, IOwnedByGuildMember
{
    [BsonId]
    public Guid Id { get; init; }

    public ulong DiscordServerId { get; init; } 

    public ulong DiscordUserId { get; init; }

    public AttendanceWarningType Type { get; init; }

    public DateTime DateStart { get; init; }

    public DateTime? DateEnd { get; init; }

    public string? PublicComment { get; init; }

    public string? PrivateComment { get; init; }

    public DateTime Created { get; init; }

    public DateTime? Updated { get; init; }

    public ulong OwnerDiscordUserId => DiscordUserId;
}
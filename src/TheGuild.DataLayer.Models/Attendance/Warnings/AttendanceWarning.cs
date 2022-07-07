using MongoDB.Bson.Serialization.Attributes;
using TheGuild.Infrastructure.MongoDb.Collections;

namespace TheGuild.DataLayer.Models.Attendance.Warnings;

[MongoCollection("AttendanceWarnings")]
public record AttendanceWarning
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
}
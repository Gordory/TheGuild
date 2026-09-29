using TheGuild.DataLayer.Attendance.Warnings;
using TheGuild.DataLayer.Models.Attendance.Warnings;

namespace TheGuild.Api.Tests.Fakes;

internal sealed class FakeAttendanceWarningRepository : IAttendanceWarningRepository
{
    private readonly List<AttendanceWarning> _warnings;

    public FakeAttendanceWarningRepository(params AttendanceWarning[] warnings)
    {
        _warnings = warnings.ToList();
    }

    /// <summary>
    /// What the service actually asked the database for. The point of the scope is that it arrives
    /// here as a filter, so the tests assert on the arguments rather than on the rows returned.
    /// </summary>
    public (ulong DiscordServerId, DateTime Date, ulong? DiscordUserId)? LastFind { get; private set; }

    public Task<ICollection<AttendanceWarning>> Find(ulong discordServerId, DateTime dateTime, ulong? discordUserId = null)
    {
        LastFind = (discordServerId, dateTime, discordUserId);

        ICollection<AttendanceWarning> found = _warnings
            .Where(warning => warning.DiscordServerId == discordServerId)
            .Where(warning => discordUserId is null || warning.DiscordUserId == discordUserId)
            .ToArray();

        return Task.FromResult(found);
    }

    public Task<AttendanceWarning?> FindAsync(ulong discordServerId, Guid id)
    {
        return Task.FromResult(_warnings
            .FirstOrDefault(warning => warning.Id == id && warning.DiscordServerId == discordServerId));
    }

    public Task<AttendanceWarning> CreateAsync(AttendanceWarning entity)
    {
        _warnings.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<AttendanceWarning> UpdateAsync(AttendanceWarning entity)
    {
        _warnings.RemoveAll(warning => warning.Id == entity.Id);
        _warnings.Add(entity);
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(Guid id)
    {
        _warnings.RemoveAll(warning => warning.Id == id);
        return Task.CompletedTask;
    }

    public Task<AttendanceWarning?> FindAsync(Guid id) => throw new NotSupportedException();

    public Task<ICollection<AttendanceWarning>> GetAllAsync() => throw new NotSupportedException();
}

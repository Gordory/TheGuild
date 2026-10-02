using TheGuild.Api.Authentication.External;
using TheGuild.Api.Authorization;
using TheGuild.DataLayer.Attendance.Warnings;
using TheGuild.DataLayer.Models.Access;
using ApiAttendanceWarning = TheGuild.Api.Models.Attendance.Warnings.AttendanceWarning;
using StoredAttendanceWarning = TheGuild.DataLayer.Models.Attendance.Warnings.AttendanceWarning;
using StoredAttendanceWarningType = TheGuild.DataLayer.Models.Attendance.Warnings.AttendanceWarningType;

namespace TheGuild.Api.Services.Attendance;

public interface IAttendanceWarningService
{
    Task<ApiAttendanceWarning?> GetAsync(GuildActor actor, Guid attendanceWarningId);

    Task<ICollection<ApiAttendanceWarning>> FindAsync(GuildActor actor, DateTime date, ulong? discordUserId);

    Task<ApiAttendanceWarning> CreateAsync(GuildActor actor, AttendanceWarningCreateRequest request);

    Task<ApiAttendanceWarning?> UpdateAsync(GuildActor actor, Guid attendanceWarningId, AttendanceWarningUpdateRequest request);

    Task<bool> DeleteAsync(GuildActor actor, Guid attendanceWarningId);
}

public sealed class AttendanceWarningService : IAttendanceWarningService
{
    private readonly IAttendanceWarningRepository _repository;
    private readonly IGuildAuthorizer _authorizer;
    private readonly IGuildMemberAccountResolver _accounts;

    public AttendanceWarningService(
        IAttendanceWarningRepository repository,
        IGuildAuthorizer authorizer,
        IGuildMemberAccountResolver accounts)
    {
        _repository = repository;
        _authorizer = authorizer;
        _accounts = accounts;
    }

    public async Task<ApiAttendanceWarning?> GetAsync(GuildActor actor, Guid attendanceWarningId)
    {
        var warning = await FindInGuildAsync(actor, attendanceWarningId);
        if (warning is null)
        {
            return null;
        }

        Require(actor, PermissionCatalog.AttendanceWarning.Read, warning);

        return warning.ToModel(actor);
    }

    public async Task<ICollection<ApiAttendanceWarning>> FindAsync(GuildActor actor, DateTime date, ulong? discordUserId)
    {
        var scope = _authorizer.ResolveScope(actor, PermissionCatalog.AttendanceWarning.Read);

        // An empty list, not a refusal: a member who may see nothing asked a question with the
        // answer "nothing", and a bot rendering a roster should not have to special-case that.
        if (scope == AccessScope.None)
        {
            return Array.Empty<ApiAttendanceWarning>();
        }

        Guid? member;

        if (scope == AccessScope.Own)
        {
            // Own narrows to the actor regardless of what was asked for, so the scope reaches Mongo as
            // a filter instead of rows being loaded and thrown away. An actor with no account owns
            // nothing, and must not fall through to an unfiltered query.
            if (actor.UserId is null)
            {
                return Array.Empty<ApiAttendanceWarning>();
            }

            member = actor.UserId;
        }
        else
        {
            // Looked up rather than created: asking about somebody is not a reason to give them an
            // account. An unknown member simply has no warnings.
            member = discordUserId is null ? null : await _accounts.FindAsync(discordUserId.Value);

            if (discordUserId is not null && member is null)
            {
                return Array.Empty<ApiAttendanceWarning>();
            }
        }

        var warnings = await _repository.Find(actor.DiscordServerId, date, member);

        return warnings.Select(warning => warning.ToModel(actor)).ToArray();
    }

    public async Task<ApiAttendanceWarning> CreateAsync(GuildActor actor, AttendanceWarningCreateRequest request)
    {
        // Decided by comparing the two people, not by resolving accounts: a refused request must not
        // leave an account behind for whoever it named.
        Require(
            actor,
            PermissionCatalog.AttendanceWarning.Create,
            isOwnRecord: request.DiscordUserId == actor.DiscordUserId);

        // Only now, past the refusal: this is the one place that gives an account to a member who has
        // never signed in, which is what lets an officer record a no-show for anybody on the server.
        var subjectUserId = await _accounts.EnsureAsync(request.DiscordUserId);

        var warning = new StoredAttendanceWarning
        {
            Id = Guid.NewGuid(),
            DiscordServerId = actor.DiscordServerId,
            UserId = subjectUserId,
            Type = (StoredAttendanceWarningType)request.Type,
            // Warnings are looked up by day, and the repository compares against a UTC date, so
            // storing the caller's time of day would make the record unfindable.
            DateStart = request.DateStart.ToUniversalTime().Date,
            DateEnd = request.DateEnd?.ToUniversalTime().Date,
            PublicComment = request.PublicComment,
            PrivateComment = request.PrivateComment,
            Created = DateTime.UtcNow,
        };

        var created = await _repository.CreateAsync(warning);

        return created.ToModel(actor);
    }

    public async Task<ApiAttendanceWarning?> UpdateAsync(
        GuildActor actor,
        Guid attendanceWarningId,
        AttendanceWarningUpdateRequest request)
    {
        var warning = await FindInGuildAsync(actor, attendanceWarningId);
        if (warning is null)
        {
            return null;
        }

        Require(actor, PermissionCatalog.AttendanceWarning.Update, warning);

        var updated = await _repository.UpdateAsync(warning with
        {
            Type = (StoredAttendanceWarningType)request.Type,
            DateStart = request.DateStart.ToUniversalTime().Date,
            DateEnd = request.DateEnd?.ToUniversalTime().Date,
            PublicComment = request.PublicComment,
            PrivateComment = request.PrivateComment,
            Updated = DateTime.UtcNow,
        });

        return updated.ToModel(actor);
    }

    public async Task<bool> DeleteAsync(GuildActor actor, Guid attendanceWarningId)
    {
        var warning = await FindInGuildAsync(actor, attendanceWarningId);
        if (warning is null)
        {
            return false;
        }

        Require(actor, PermissionCatalog.AttendanceWarning.Delete, warning);

        await _repository.DeleteAsync(attendanceWarningId);

        return true;
    }

    private Task<StoredAttendanceWarning?> FindInGuildAsync(GuildActor actor, Guid attendanceWarningId)
    {
        // The guild-scoped overload on purpose: an id alone is guessable across guilds.
        return _repository.FindAsync(actor.DiscordServerId, attendanceWarningId);
    }

    private void Require(GuildActor actor, PermissionPair permission, IOwnedByGuildMember resource)
    {
        Require(actor, permission, resource.OwnerUserId == actor.UserId);
    }

    private void Require(GuildActor actor, PermissionPair permission, bool isOwnRecord)
    {
        if (_authorizer.IsAllowedFor(actor, permission, isOwnRecord))
        {
            return;
        }

        // Name the permission that would have covered this request, not the one for own records: the
        // member is being refused because the record is someone else's.
        throw new GuildAccessDeniedException(isOwnRecord ? permission.Own : permission.Any);
    }
}

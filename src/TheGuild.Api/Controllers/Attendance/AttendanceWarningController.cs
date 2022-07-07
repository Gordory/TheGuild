using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGuild.Api.Authentication;
using TheGuild.Api.Authorization;
using TheGuild.Api.Models.Attendance.Warnings;
using TheGuild.Api.Services.Attendance;

namespace TheGuild.Api.Controllers.Attendance;

[Authorize]
[ApiController]
[Route("{serverId}/attendance/warning/")]
public class AttendanceWarningController : ControllerBase
{
    private readonly IAttendanceWarningService _service;
    private readonly IAuthenticationChecker _authenticationChecker;
    private readonly IAuthorizedGuildUserProvider _authorizedGuildUserProvider;

    public AttendanceWarningController(
        IAttendanceWarningService service,
        IAuthenticationChecker authenticationChecker,
        IAuthorizedGuildUserProvider authorizedGuildUserProvider)
    {
        _service = service;
        _authenticationChecker = authenticationChecker;
        _authorizedGuildUserProvider = authorizedGuildUserProvider;
    }

    [HttpGet("{attendanceWarningId:guid}")]
    public async Task<AttendanceWarning> Get(ulong serverId, Guid attendanceWarningId)
    {
        await _authenticationChecker.EnsureUserIsAuthenticatedAsync(User);
        var authorizedUser = await _authorizedGuildUserProvider.GetAsync(serverId, User);

        return await _service.GetAsync(authorizedUser, attendanceWarningId);
    }

    [HttpGet]
    public async Task<ICollection<AttendanceWarning>> Find(ulong serverId, DateTime? dateTime)
    {
        await _authenticationChecker.EnsureUserIsAuthenticatedAsync(User);
        var authorizedUser = await _authorizedGuildUserProvider.GetAsync(serverId, User);

        return await _service.FindAsync(authorizedUser, dateTime);
    }

    [HttpPost]
    public async Task<AttendanceWarning> Create(ulong serverId, AttendanceWarningCreateRequest attendanceWarningCreateRequest)
    {
        await _authenticationChecker.EnsureUserIsAuthenticatedAsync(User);
        var authorizedUser = await _authorizedGuildUserProvider.GetAsync(serverId, User);

        return await _service.CreateAsync(authorizedUser, attendanceWarningCreateRequest);
    }

    [HttpPut("{attendanceWarningId:guid}")]
    public async Task<AttendanceWarning> Update(
        ulong serverId, 
        Guid attendanceWarningId,
        AttendanceWarningUpdateRequest attendanceWarningUpdateRequest)
    {
        await _authenticationChecker.EnsureUserIsAuthenticatedAsync(User);
        var authorizedUser = await _authorizedGuildUserProvider.GetAsync(serverId, User);

        return await _service.UpdateAsync(authorizedUser, attendanceWarningId, attendanceWarningUpdateRequest);
    }

    [HttpDelete("{attendanceWarningId:guid}")]
    public async Task<AttendanceWarning> Delete(
        ulong serverId, 
        Guid attendanceWarningId)
    {
        await _authenticationChecker.EnsureUserIsAuthenticatedAsync(User);
        var authorizedUser = await _authorizedGuildUserProvider.GetAsync(serverId, User);

        return await _service.DeleteAsync(authorizedUser, attendanceWarningId);
    }
}
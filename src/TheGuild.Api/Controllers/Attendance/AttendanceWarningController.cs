using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGuild.Api.Authorization;
using TheGuild.Api.Models.Attendance.Warnings;
using TheGuild.Api.Services.Attendance;

namespace TheGuild.Api.Controllers.Attendance;

[Authorize]
[ApiController]
[Route("{serverId}/attendance/warning")]
public class AttendanceWarningController : ControllerBase
{
    private readonly IAttendanceWarningService _service;
    private readonly IGuildActorAccessor _guildActorAccessor;

    public AttendanceWarningController(
        IAttendanceWarningService service,
        IGuildActorAccessor guildActorAccessor)
    {
        _service = service;
        _guildActorAccessor = guildActorAccessor;
    }

    [HttpGet("{attendanceWarningId:guid}")]
    public async Task<ActionResult<AttendanceWarning>> Get(ulong serverId, Guid attendanceWarningId)
    {
        var actor = await GetActorAsync(serverId);

        var warning = await _service.GetAsync(actor, attendanceWarningId);

        return warning is null ? NotFound() : warning;
    }

    [HttpGet]
    public async Task<ICollection<AttendanceWarning>> Find(ulong serverId, DateTime? date, ulong? discordUserId)
    {
        var actor = await GetActorAsync(serverId);

        return await _service.FindAsync(actor, date ?? DateTime.UtcNow, discordUserId);
    }

    [HttpPost]
    public async Task<AttendanceWarning> Create(ulong serverId, AttendanceWarningCreateRequest request)
    {
        var actor = await GetActorAsync(serverId);

        return await _service.CreateAsync(actor, request);
    }

    [HttpPut("{attendanceWarningId:guid}")]
    public async Task<ActionResult<AttendanceWarning>> Update(
        ulong serverId,
        Guid attendanceWarningId,
        AttendanceWarningUpdateRequest request)
    {
        var actor = await GetActorAsync(serverId);

        var warning = await _service.UpdateAsync(actor, attendanceWarningId, request);

        return warning is null ? NotFound() : warning;
    }

    [HttpDelete("{attendanceWarningId:guid}")]
    public async Task<IActionResult> Delete(ulong serverId, Guid attendanceWarningId)
    {
        var actor = await GetActorAsync(serverId);

        return await _service.DeleteAsync(actor, attendanceWarningId) ? NoContent() : NotFound();
    }

    private Task<GuildActor> GetActorAsync(ulong serverId)
    {
        return _guildActorAccessor.GetAsync(serverId, User);
    }
}

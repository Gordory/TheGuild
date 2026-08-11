using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGuild.DataLayer.Attendance.Warnings;
using TheGuild.DataLayer.Authentication;
using TheGuild.DataLayer.Models.Attendance.Warnings;
using TheGuild.DataLayer.Models.Authentication;

namespace TheGuild.Api.Controllers.Attendance;

[Authorize]
[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly IAttendanceWarningRepository _attendanceWarningRepository;
    private readonly IApiKeyBindingRepository _apiKeyBindingRepository;

    public TestController(
        IAttendanceWarningRepository attendanceWarningRepository,
        IApiKeyBindingRepository apiKeyBindingRepository)
    {
        _attendanceWarningRepository = attendanceWarningRepository;
        _apiKeyBindingRepository = apiKeyBindingRepository;
    }

    [HttpPost]
    public async Task TestAsync(ulong serverId, Guid? attendanceWarningId)
    {
        if (attendanceWarningId != null)
        {
            await _attendanceWarningRepository.DeleteAsync(attendanceWarningId.Value);
            return;
        }

        var entity = new AttendanceWarning
        {
            Id = new Guid("e7dc9817-2bd2-4752-ac3d-4a0a5894533b"),
            DateStart = DateTime.UtcNow.Date,
            DiscordServerId = serverId,
            DiscordUserId = 165505414476201984,
            PublicComment = "Public",
            PrivateComment = "Private",
            Type = AttendanceWarningType.Late
        };

        var newEntity = await _attendanceWarningRepository.CreateAsync(entity);
        return;
    }

    [HttpGet]
    public async Task<AttendanceWarning> TestAsync()
    {
        var entity = new AttendanceWarning
        {
            Id = new Guid("e7dc9817-2bd2-4752-ac3d-4a0a5894533b"),
            DateStart = DateTime.UtcNow.Date,
            DiscordServerId = 123,
            DiscordUserId = 165505414476201984,
            PublicComment = "Public",
            PrivateComment = "Private",
            Type = AttendanceWarningType.Late
        };
        return entity;
    }
}
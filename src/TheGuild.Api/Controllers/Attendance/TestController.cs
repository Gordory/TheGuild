using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheGuild.DataLayer.Attendance.Warnings;

namespace TheGuild.Api.Controllers.Attendance;

[Authorize]
[ApiController]
[Route("test")]
public class TestController
{
    private readonly IAttendanceWarningRepository _attendanceWarningRepository;

    public TestController(IAttendanceWarningRepository attendanceWarningRepository)
    {
        _attendanceWarningRepository = attendanceWarningRepository;
    }

    [HttpPost]
    public async Task TestAsync(ulong serverId, Guid? attendanceWarningId)
    {
        if (attendanceWarningId != null)
        {
            await _attendanceWarningRepository.DeleteAsync(attendanceWarningId.Value);
            return;
        }

        var entity = new DataLayer.Models.Attendance.Warnings.AttendanceWarning
        {
            Id = new Guid("e7dc9817-2bd2-4752-ac3d-4a0a5894533b"),
            DateStart = DateTime.UtcNow.Date,
            DiscordServerId = serverId,
            DiscordUserId = 165505414476201984,
            PublicComment = "Public",
            PrivateComment = "Private",
            Type = DataLayer.Models.Attendance.Warnings.AttendanceWarningType.Late
        };

        var newEntity = await _attendanceWarningRepository.CreateAsync(entity);
        return;
    }
}
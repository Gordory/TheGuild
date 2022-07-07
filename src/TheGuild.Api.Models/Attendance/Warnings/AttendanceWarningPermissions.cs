namespace TheGuild.Api.Models.Attendance.Warnings;

[Flags]
public enum AttendanceWarningPermissions : int
{
    CreateSelf = 1,             // 0000 0000 0000 0001
    CreateForOthers = 2,        // 0000 0000 0000 0011
    ReadSelf = 16,              // 0000 0000 0001 0000
    ReadAll = 48,               // 0000 0000 0011 0000
    ReadPrivateComments = 64,   // 0000 0000 0100 0000
    UpdateSelf = 256,           // 0000 0001 0000 0000
    UpdateForOthers = 768,      // 0000 0011 0000 0000
    DeleteSelf = 4096,          // 0001 0000 0000 0000
    DeleteForOthers = 12288,    // 0011 0000 0000 0000
}
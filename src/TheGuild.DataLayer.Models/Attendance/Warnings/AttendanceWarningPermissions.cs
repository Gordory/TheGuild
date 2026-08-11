namespace TheGuild.DataLayer.Models.Attendance.Warnings;

[Flags]
public enum AttendanceWarningPermissions : uint
{
    None = 0,                   // 0000 0000 0000 0000
    CreateSelf = 1,             // 0000 0000 0000 0001
    CreateForOthers = 2,        // 0000 0000 0000 0010
    ReadSelf = 16,              // 0000 0000 0001 0000
    ReadForOthers = 32,         // 0000 0000 0010 0000
    ReadPrivateComments = 64,   // 0000 0000 0100 0000
    UpdateSelf = 256,           // 0000 0001 0000 0000
    UpdateForOthers = 512,      // 0000 0010 0000 0000
    DeleteSelf = 4096,          // 0001 0000 0000 0000
    DeleteForOthers = 8192,     // 0010 0000 0000 0000
}
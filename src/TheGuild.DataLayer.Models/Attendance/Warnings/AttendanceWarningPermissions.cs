namespace TheGuild.DataLayer.Models.Attendance.Warnings;

// Each capability owns exactly one bit, so a stored value never carries a meaning nobody granted.
// The "All" members are compiler-computed unions rather than hand-written constants: spelling the
// combined value out by hand is how CreateForOthers once ended up as 2 while claiming to be 3.
// Verbs are spaced apart to leave room for capabilities that do not exist yet.
[Flags]
public enum AttendanceWarningPermissions : int
{
    None = 0,                                   // 0000 0000 0000 0000

    CreateSelf = 1,                             // 0000 0000 0000 0001
    CreateForOthers = 2,                        // 0000 0000 0000 0010
    CreateAll = CreateSelf | CreateForOthers,   // 0000 0000 0000 0011

    ReadSelf = 16,                              // 0000 0000 0001 0000
    ReadForOthers = 32,                         // 0000 0000 0010 0000
    ReadAll = ReadSelf | ReadForOthers,         // 0000 0000 0011 0000

    // Kept out of ReadAll on purpose: being allowed to see a warning is not the same as being
    // allowed to see what an officer wrote about it privately.
    ReadPrivateComments = 64,                   // 0000 0000 0100 0000

    UpdateSelf = 256,                           // 0000 0001 0000 0000
    UpdateForOthers = 512,                      // 0000 0010 0000 0000
    UpdateAll = UpdateSelf | UpdateForOthers,   // 0000 0011 0000 0000

    DeleteSelf = 4096,                          // 0001 0000 0000 0000
    DeleteForOthers = 8192,                     // 0010 0000 0000 0000
    DeleteAll = DeleteSelf | DeleteForOthers,   // 0011 0000 0000 0000
}

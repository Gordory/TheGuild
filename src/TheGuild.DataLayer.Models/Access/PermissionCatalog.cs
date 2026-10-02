namespace TheGuild.DataLayer.Models.Access;

/// <summary>
/// Every permission the application knows how to enforce. Roles store ids from here; the database
/// carries a projection of this catalogue for the settings screen, never the other way round, so a
/// row edited in the database cannot quietly disagree with the code that does the checking.
/// </summary>
public static class PermissionCatalog
{
    public static class AttendanceWarning
    {
        public const string Resource = "attendance.warning";

        public const string ReadOwn = "attendance.warning.read.own";
        public const string ReadAny = "attendance.warning.read.any";
        public const string CreateOwn = "attendance.warning.create.own";
        public const string CreateAny = "attendance.warning.create.any";
        public const string UpdateOwn = "attendance.warning.update.own";
        public const string UpdateAny = "attendance.warning.update.any";
        public const string DeleteOwn = "attendance.warning.delete.own";
        public const string DeleteAny = "attendance.warning.delete.any";

        /// <summary>
        /// Deliberately outside every <see cref="Permission.Implies"/> chain: being allowed to see a
        /// warning is not being allowed to see what an officer wrote about it privately.
        /// </summary>
        public const string ReadPrivate = "attendance.warning.private.read";

        public static readonly PermissionPair Read = new(ReadOwn, ReadAny);
        public static readonly PermissionPair Create = new(CreateOwn, CreateAny);
        public static readonly PermissionPair Update = new(UpdateOwn, UpdateAny);
        public static readonly PermissionPair Delete = new(DeleteOwn, DeleteAny);
    }

    private static readonly Permission[] Declared =
    {
        new(AttendanceWarning.ReadOwn, AttendanceWarning.Resource, "read.own"),
        new(AttendanceWarning.ReadAny, AttendanceWarning.Resource, "read.any")
        {
            Implies = new[] { AttendanceWarning.ReadOwn },
        },
        new(AttendanceWarning.CreateOwn, AttendanceWarning.Resource, "create.own"),
        new(AttendanceWarning.CreateAny, AttendanceWarning.Resource, "create.any")
        {
            Implies = new[] { AttendanceWarning.CreateOwn },
        },
        new(AttendanceWarning.UpdateOwn, AttendanceWarning.Resource, "update.own"),
        new(AttendanceWarning.UpdateAny, AttendanceWarning.Resource, "update.any")
        {
            Implies = new[] { AttendanceWarning.UpdateOwn },
        },
        new(AttendanceWarning.DeleteOwn, AttendanceWarning.Resource, "delete.own"),
        new(AttendanceWarning.DeleteAny, AttendanceWarning.Resource, "delete.any")
        {
            Implies = new[] { AttendanceWarning.DeleteOwn },
        },
        new(AttendanceWarning.ReadPrivate, AttendanceWarning.Resource, "private.read"),
    };

    public static IReadOnlyDictionary<string, Permission> ById { get; } =
        Declared.ToDictionary(permission => permission.Id);

    public static IReadOnlyCollection<Permission> All => Declared;

    public static bool Contains(string permissionId) => ById.ContainsKey(permissionId);

    /// <summary>
    /// Closes a granted set over <see cref="Permission.Implies"/>. Ids the catalogue no longer
    /// declares are passed through untouched rather than dropped: a stored grant that lost its
    /// declaration matches no check anyway, and silently discarding it would hide the drift.
    /// </summary>
    public static IReadOnlySet<string> Expand(IEnumerable<string> permissionIds)
    {
        var expanded = new HashSet<string>();
        var pending = new Stack<string>(permissionIds);

        while (pending.Count > 0)
        {
            var permissionId = pending.Pop();

            // Also the cycle guard: an id already seen is never walked a second time.
            if (!expanded.Add(permissionId) || !ById.TryGetValue(permissionId, out var permission))
            {
                continue;
            }

            foreach (var implied in permission.Implies)
            {
                pending.Push(implied);
            }
        }

        return expanded;
    }
}

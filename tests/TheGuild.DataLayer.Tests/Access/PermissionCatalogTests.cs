using System.Text.RegularExpressions;
using TheGuild.DataLayer.Models.Access;
using Xunit;

namespace TheGuild.DataLayer.Tests.Access;

/// <summary>
/// The catalogue is hand-written, and a typo in it produces a permission nobody can ever hold
/// rather than a compile error. These tests are what makes that impossible.
/// </summary>
public class PermissionCatalogTests
{
    private static readonly Regex IdFormat = new("^[a-z][a-z0-9-]*(\\.[a-z][a-z0-9-]*)+$");

    [Fact]
    public void Every_id_is_declared_once()
    {
        var duplicates = PermissionCatalog.All
            .GroupBy(permission => permission.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        Assert.Empty(duplicates);
    }

    [Fact]
    public void Every_id_follows_the_dotted_lowercase_format()
    {
        var malformed = PermissionCatalog.All
            .Select(permission => permission.Id)
            .Where(id => !IdFormat.IsMatch(id));

        Assert.Empty(malformed);
    }

    [Fact]
    public void Every_id_starts_with_its_resource_and_ends_with_its_action()
    {
        Assert.All(PermissionCatalog.All, permission =>
            Assert.Equal($"{permission.Resource}.{permission.Action}", permission.Id));
    }

    [Fact]
    public void Every_implied_permission_is_declared()
    {
        var dangling = PermissionCatalog.All
            .SelectMany(permission => permission.Implies)
            .Where(implied => !PermissionCatalog.Contains(implied));

        Assert.Empty(dangling);
    }

    [Fact]
    public void Expansion_adds_implied_permissions()
    {
        var expanded = PermissionCatalog.Expand(new[] { PermissionCatalog.AttendanceWarning.ReadAny });

        Assert.Contains(PermissionCatalog.AttendanceWarning.ReadOwn, expanded);
        Assert.Contains(PermissionCatalog.AttendanceWarning.ReadAny, expanded);
    }

    [Fact]
    public void Expansion_does_not_grant_reading_private_comments()
    {
        var expanded = PermissionCatalog.Expand(PermissionCatalog.All
            .Select(permission => permission.Id)
            .Where(id => id != PermissionCatalog.AttendanceWarning.ReadPrivate));

        Assert.DoesNotContain(PermissionCatalog.AttendanceWarning.ReadPrivate, expanded);
    }

    [Fact]
    public void Expansion_keeps_an_id_the_catalogue_no_longer_declares()
    {
        var expanded = PermissionCatalog.Expand(new[] { "raid.signup.manage" });

        Assert.Equal(new[] { "raid.signup.manage" }, expanded);
    }

    [Fact]
    public void Expansion_terminates_on_a_cycle()
    {
        // Guards the traversal itself: the declared catalogue is acyclic, so a regression would
        // otherwise only show up as a hang once someone writes a cycle into it.
        var expanded = PermissionCatalog.Expand(new[]
        {
            PermissionCatalog.AttendanceWarning.ReadAny,
            PermissionCatalog.AttendanceWarning.ReadOwn,
            PermissionCatalog.AttendanceWarning.ReadAny,
        });

        Assert.Equal(2, expanded.Count);
    }

    [Fact]
    public void Expansion_of_nothing_is_empty()
    {
        Assert.Empty(PermissionCatalog.Expand(Array.Empty<string>()));
    }
}

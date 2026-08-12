using Xunit;
using ApiPermissions = TheGuild.Api.Models.Attendance.Warnings.AttendanceWarningPermissions;
using DataLayerPermissions = TheGuild.DataLayer.Models.Attendance.Warnings.AttendanceWarningPermissions;

namespace TheGuild.DataLayer.Tests.Attendance.Warnings;

/// <summary>
/// The API layer and the data layer declare the permission set twice and map between them.
/// These tests are what keeps the two copies from drifting apart.
/// </summary>
public class AttendanceWarningPermissionsTests
{
    [Fact]
    public void Both_layers_share_an_underlying_type()
    {
        Assert.Equal(
            Enum.GetUnderlyingType(typeof(DataLayerPermissions)),
            Enum.GetUnderlyingType(typeof(ApiPermissions)));
    }

    [Fact]
    public void Both_layers_declare_the_same_members()
    {
        Assert.Equal(Members<DataLayerPermissions>(), Members<ApiPermissions>());
    }

    [Theory]
    [InlineData(DataLayerPermissions.CreateAll, DataLayerPermissions.CreateSelf, DataLayerPermissions.CreateForOthers)]
    [InlineData(DataLayerPermissions.ReadAll, DataLayerPermissions.ReadSelf, DataLayerPermissions.ReadForOthers)]
    [InlineData(DataLayerPermissions.UpdateAll, DataLayerPermissions.UpdateSelf, DataLayerPermissions.UpdateForOthers)]
    [InlineData(DataLayerPermissions.DeleteAll, DataLayerPermissions.DeleteSelf, DataLayerPermissions.DeleteForOthers)]
    public void A_composite_member_is_exactly_the_union_of_its_parts(
        DataLayerPermissions composite,
        DataLayerPermissions self,
        DataLayerPermissions forOthers)
    {
        Assert.Equal(self | forOthers, composite);
        Assert.True(composite.HasFlag(self));
        Assert.True(composite.HasFlag(forOthers));
    }

    [Fact]
    public void Reading_every_warning_does_not_grant_reading_private_comments()
    {
        Assert.False(DataLayerPermissions.ReadAll.HasFlag(DataLayerPermissions.ReadPrivateComments));
    }

    // Keyed by name rather than by value: composite members share their bits with the members they
    // combine, and ToString() on such a value is free to render either name.
    private static IDictionary<string, long> Members<TEnum>()
        where TEnum : struct, Enum
    {
        return Enum.GetNames<TEnum>()
            .ToDictionary(
                name => name,
                name => Convert.ToInt64(Enum.Parse<TEnum>(name)));
    }
}

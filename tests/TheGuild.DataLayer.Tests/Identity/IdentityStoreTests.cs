using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TheGuild.DataLayer.Models.Identity;
using Xunit;

namespace TheGuild.DataLayer.Tests.Identity;

/// <summary>
/// The account model rests on two things the compiler cannot check: that the Mongo store can carry a
/// Guid key, and that an account is findable by the external login attached to it. Everything else —
/// sign-in, linking, recording a warning against someone who never signed in — is built on those.
/// </summary>
public class IdentityStoreTests : IClassFixture<MongoContainerFixture>, IAsyncLifetime
{
    private const string Discord = "Discord";
    private const string BattleNet = "BattleNet";

    private readonly MongoContainerFixture _mongo;
    private readonly ServiceProvider _services;

    public IdentityStoreTests(MongoContainerFixture mongo)
    {
        _mongo = mongo;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole, Guid>(options =>
        {
            options.ConnectionString = _mongo.IdentityConnectionString;
            options.UsersCollection = "Identity.Users";
            options.RolesCollection = "Identity.Roles";
            options.MigrationCollection = "Identity.Migrations";
        });
        services.Configure<IdentityOptions>(ApplicationIdentityOptions.Configure);

        _services = services.BuildServiceProvider();
    }

    private UserManager<ApplicationUser> Users =>
        _services.GetRequiredService<UserManager<ApplicationUser>>();

    public async Task InitializeAsync()
    {
        await _mongo.Database.DropCollectionAsync("Identity.Users");
    }

    public Task DisposeAsync()
    {
        _services.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task An_account_keyed_by_a_guid_round_trips()
    {
        var user = Account();

        var created = await Users.CreateAsync(user);
        Assert.True(created.Succeeded, Describe(created));

        var found = await Users.FindByIdAsync(user.Id.ToString());

        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public async Task An_account_is_found_by_the_external_login_attached_to_it()
    {
        var user = Account();
        await Users.CreateAsync(user);

        var linked = await Users.AddLoginAsync(user, new UserLoginInfo(Discord, "165505414476201984", Discord));
        Assert.True(linked.Succeeded, Describe(linked));

        var found = await Users.FindByLoginAsync(Discord, "165505414476201984");

        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public async Task Several_providers_lead_to_the_same_account()
    {
        var user = Account();
        await Users.CreateAsync(user);
        await Users.AddLoginAsync(user, new UserLoginInfo(Discord, "discord-key", Discord));
        await Users.AddLoginAsync(user, new UserLoginInfo(BattleNet, "battlenet-key", BattleNet));

        var viaDiscord = await Users.FindByLoginAsync(Discord, "discord-key");
        var viaBattleNet = await Users.FindByLoginAsync(BattleNet, "battlenet-key");

        Assert.Equal(user.Id, viaDiscord!.Id);
        Assert.Equal(user.Id, viaBattleNet!.Id);
    }

    [Fact]
    public async Task An_account_is_found_by_email_so_linking_can_be_offered()
    {
        var user = Account(email: "officer@example.com");
        await Users.CreateAsync(user);

        var found = await Users.FindByEmailAsync("officer@example.com");

        Assert.NotNull(found);
        Assert.Equal(user.Id, found!.Id);
    }

    [Fact]
    public async Task An_unknown_login_finds_nothing_rather_than_failing()
    {
        Assert.Null(await Users.FindByLoginAsync(Discord, "never-seen"));
    }

    private static ApplicationUser Account(string? email = null)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"Discord:{Guid.NewGuid()}",
            Email = email,
            EmailConfirmed = email is not null,
            DisplayName = "Officer",
        };
    }

    private static string Describe(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(error => error.Description));
    }
}

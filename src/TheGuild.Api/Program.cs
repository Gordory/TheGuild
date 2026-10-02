using AspNetCore.Identity.Mongo;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using TheGuild.Api.Access;
using TheGuild.Api.Authentication.ApiKey;
using TheGuild.Api.Authentication.External;
using TheGuild.Api.Authorization;
using TheGuild.Api.Services.Attendance;
using TheGuild.DataLayer.Access;
using TheGuild.DataLayer.Attendance.Warnings;
using TheGuild.DataLayer.Authentication;
using TheGuild.DataLayer.Guilds;
using TheGuild.DataLayer.Models.Identity;
using TheGuild.External.Discord;
using TheGuild.Infrastructure.MongoDb;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Configuration;

var builder = WebApplication.CreateBuilder(args);

var mongoDbConfigurationSection = builder.Configuration.GetSection(MongoDbConfiguration.ConfigPath);
var mongoDbConfiguration = mongoDbConfigurationSection.Get<MongoDbConfiguration>();
builder.Services.Configure<MongoDbConfiguration>(mongoDbConfigurationSection);

builder.Services.AddTransient<IAttendanceWarningService, AttendanceWarningService>();
builder.Services.AddTransient<IAttendanceWarningRepository, AttendanceWarningRepository>();
builder.Services.AddSingleton<IApiKeyBindingRepository, ApiKeyBindingRepository>();
builder.Services.AddSingleton<IGuildRepository, GuildRepository>();
builder.Services.AddSingleton<IPermissionDescriptorRepository, PermissionDescriptorRepository>();
builder.Services.AddHostedService<PermissionCatalogProjection>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IGuildMemberRolesReader, DiscordGuildMemberRolesReader>();
builder.Services.AddSingleton<IGuildActorProvider, GuildActorProvider>();
builder.Services.AddSingleton<IGuildAuthorizer, GuildAuthorizer>();
builder.Services.AddSingleton<IGuildActorAccessor, GuildActorAccessor>();
builder.Services.AddScoped<IGuildMemberAccountResolver, GuildMemberAccountResolver>();
builder.Services.AddDiscordClient(builder.Configuration);
builder.Services.AddSingleton<IMongoClientProvider, MongoClientProvider>();
builder.Services.AddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();
builder.Services.AddSingleton<IMongoDatabase>(x => x.GetService<IMongoDatabaseProvider>()!.Get());
builder.Services.AddSingleton<IMongoDbCollectionNamesCache, MongoDbCollectionNamesCache>();
// Add services to the container.

// Accounts are ours; external providers are only ways into them. Registered before the
// authentication schemes because AddIdentity installs its own cookies and defaults, which the lines
// below then adjust rather than fight.
builder.Services.AddIdentityMongoDbProvider<ApplicationUser, ApplicationRole, Guid>(mongo =>
{
    mongo.ConnectionString = mongoDbConfiguration!.ConnectionString;
    mongo.UsersCollection = "Identity.Users";
    mongo.RolesCollection = "Identity.Roles";
    mongo.MigrationCollection = "Identity.Migrations";
});

builder.Services.Configure<IdentityOptions>(ApplicationIdentityOptions.Configure);

builder.Services
    .AddAuthentication(options =>
    {
        // Most traffic is a service client with an API key; a browser session is read from Identity's
        // cookie where an endpoint asks for it by policy.
        options.DefaultChallengeScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
        // Deliberately left where AddIdentity put it: the external handshake signs into Identity's
        // external cookie, and GetExternalLoginInfoAsync reads the pending identity from there.
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddDiscord(options =>
    {
        options.ClientId = builder.Configuration.GetValue<string>("Discord:OAuth2:ClientId");
        options.ClientSecret = builder.Configuration.GetValue<string>("Discord:OAuth2:ClientSecret");
        options.SaveTokens = true;

        // Without this scope Discord returns no address at all, and the account-linking offer has
        // nothing to match on.
        options.Scope.Add("email");

        // The provider package maps no verified flag of its own. Linking on an unverified address
        // would let someone register a Discord account on another person's email and claim their
        // account here, so the raw field is mapped and checked.
        options.ClaimActions.MapJsonKey(ExternalLoginClaims.EmailVerified, "verified");
    })
    .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationDefaults.AuthenticationScheme,
        ApiKeyAuthenticationDefaults.DisplayName,
        options =>
        {
            options.ClaimsIssuer = "TheGuild.Api";
        });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            IdentityConstants.ApplicationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers(options => options.Filters.Add<GuildAccessDeniedExceptionFilter>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
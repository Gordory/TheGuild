using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using TheGuild.Api.Authentication;
using TheGuild.Api.Authentication.ApiKey;
using TheGuild.Api.Authorization;
using TheGuild.DataLayer.Attendance.Warnings;
using TheGuild.DataLayer.Authentication;
using TheGuild.DataLayer.Guilds;
using TheGuild.External.Discord;
using TheGuild.Infrastructure.MongoDb;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Configuration;

var builder = WebApplication.CreateBuilder(args);

var mongoDbConfigurationSection = builder.Configuration.GetSection(MongoDbConfiguration.ConfigPath);
var mongoDbConfiguration = mongoDbConfigurationSection.Get<MongoDbConfiguration>();
builder.Services.Configure<MongoDbConfiguration>(mongoDbConfigurationSection);

//builder.Services.AddTransient<IAttendanceWarningService, AttendanceWarningService>();
builder.Services.AddTransient<IAttendanceWarningRepository, AttendanceWarningRepository>();
builder.Services.AddSingleton<IApiKeyBindingRepository, ApiKeyBindingRepository>();
builder.Services.AddSingleton<IGuildRepository, GuildRepository>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IGuildMemberRolesReader, DiscordGuildMemberRolesReader>();
builder.Services.AddSingleton<IGuildActorProvider, GuildActorProvider>();
builder.Services.AddSingleton<IGuildAuthorizer, GuildAuthorizer>();
builder.Services.AddSingleton<IGuildActorAccessor, GuildActorAccessor>();
builder.Services.AddDiscordClient(builder.Configuration);
builder.Services.AddSingleton<IMongoClientProvider, MongoClientProvider>();
builder.Services.AddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();
builder.Services.AddSingleton<IMongoDatabase>(x => x.GetService<IMongoDatabaseProvider>()!.Get());
builder.Services.AddSingleton<IMongoDbCollectionNamesCache, MongoDbCollectionNamesCache>();
// Add services to the container.

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddDiscord(options =>
    {
        options.ClientId = builder.Configuration.GetValue<string>("Discord:OAuth2:ClientId");
        options.ClientSecret = builder.Configuration.GetValue<string>("Discord:OAuth2:ClientSecret");
        options.SaveTokens = true;
    })
    .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationDefaults.AuthenticationScheme,
        ApiKeyAuthenticationDefaults.DisplayName,
        options =>
        {
            options.ClaimsIssuer = "TheGuild.Api";
        })
    .AddCookie();

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(
            ApiKeyAuthenticationDefaults.AuthenticationScheme,
            DiscordAuthenticationDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddControllers();
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
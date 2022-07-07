using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;
using TheGuild.Api.Authentication;
using TheGuild.Api.Authorization;
using TheGuild.Api.Services.Attendance;
using TheGuild.DataLayer.Attendance.Warnings;
using TheGuild.Infrastructure.MongoDb;
using TheGuild.Infrastructure.MongoDb.Collections;
using TheGuild.Infrastructure.MongoDb.Configuration;

var builder = WebApplication.CreateBuilder(args);

var mongoDbConfigurationSection = builder.Configuration.GetSection(MongoDbConfiguration.ConfigPath);
var mongoDbConfiguration = mongoDbConfigurationSection.Get<MongoDbConfiguration>();
builder.Services.Configure<MongoDbConfiguration>(mongoDbConfigurationSection);

builder.Services.AddTransient<IAttendanceWarningService, AttendanceWarningService>();
builder.Services.AddTransient<IAuthenticationChecker, AuthenticationChecker>();
builder.Services.AddTransient<IAuthorizedGuildUserProvider, AuthorizedGuildUserProvider>();
builder.Services.AddTransient<IAttendanceWarningRepository, AttendanceWarningRepository>();
builder.Services.AddSingleton<IMongoClientProvider, MongoClientProvider>();
builder.Services.AddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();
builder.Services.AddSingleton<IMongoDatabase>(x => x.GetService<IMongoDatabaseProvider>()!.Get());
builder.Services.AddSingleton<IMongoDbCollectionNamesCache, MongoDbCollectionNamesCache>();
// Add services to the container.

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddDiscord(options =>
    {
        options.ClientId = builder.Configuration.GetValue<string>("Discord:OAuth2:ClientId");
        options.ClientSecret = builder.Configuration.GetValue<string>("Discord:OAuth2:ClientSecret");
        options.SaveTokens = true;
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
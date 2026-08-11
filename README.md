# TheGuild

**TheGuild** is a guild management platform for World of Warcraft. The backend service exposes a REST API for member records, raid attendance, roster and guild bank. A Discord integration lets officers run the guild straight from their server, without third-party tools.

> ⚠️ **Status: early development.** The service is not usable yet. What follows describes the code as it actually stands, not where it is headed.

> The project follows a freemium model: core features are free, advanced capabilities come with a subscription.

## Current state

### Working

- **API key authentication** — an `X-API-KEY` header carrying a GUID. The key is looked up in MongoDB and can be bound to a Discord bot (`DiscordBotApiKeyBinding`); disabled keys (`Enabled: false`) are rejected. The scheme is registered and used as the default.
- **Domain models and storage** — `AttendanceWarning` (types `Absence` / `Late`, public and private comment), `Guild` / `GuildRole` / `GuildPermissions`, and MongoDB repositories on top of a shared CRUD base.
- **Permission resolution** — `GuildPermissionsProvider` merges a member's permissions from guild roles bound to Discord roles and from per-user bindings, reading the member's roles from the Discord API.
- **Discord.Net wrapper** — `IDiscordClientFactory` builds a `DiscordRestClient` from a bot token (`Discord:Bot:Token`).

### Not working yet

- **Attendance API** — `AttendanceWarningController` and the `IAttendanceWarningService` registration are commented out while the authorization layer is being reworked. There are no public attendance endpoints right now.
- **Discord OAuth2** — the provider is wired up (`AspNet.Security.OAuth.Discord` plus a cookie scheme), but there are no sign-in or callback endpoints, so the user login flow is unavailable.
- **Discord bot** — `TheGuild.External.Discord` registers through LightInject and is not yet plugged into the API host; the configuration has no `Discord:Bot` section.
- **`TestController`** (`POST/GET /test`) — temporary scaffolding for exercising the repository by hand, not part of the API.

## Roadmap

Ordered by dependency rather than by date — there are no committed timelines.

### Foundation — in progress

Everything below this section is blocked on it.

- Finish the authorization rework and bring the attendance API back online
- Register the permission and guild dependencies in the API host, and plug `TheGuild.External.Discord` into it
- Tests for permission resolution — a mistake there leaks private comments between members
- Per-guild data isolation enforced on every repository query
- CI, plus a `Dockerfile` and Compose setup for local runs

### Next — the core loop

- **Guild roster** — members, classes, roles. Everything else depends on it: attendance currently stores a bare `DiscordUserId` with no member entity behind it
- **Attendance** — endpoints rebuilt on top of the roster
- **Discord bot** — run the guild through slash commands, including automated channel notifications

### Later

- **Raid management** — sign-ups, compositions, raid history
- **Guild bank and loot** — item tracking and loot distribution
- **Subscriptions** — billing, plan entitlements and free-tier limits behind the freemium model
- **Guild Armory** — public guild page
- **Companion clients** — mobile app with push notifications, desktop app
- **WoW addon** — the addon writes to `SavedVariables` and the desktop companion uploads the file; the addon API has no network access, so in-game data cannot reach the service on its own

## Architecture

```
TheGuild.sln
├── src/
│   ├── TheGuild.Api               # ASP.NET Core 6 REST API
│   ├── TheGuild.Api.Models        # API-layer DTOs and models
│   ├── TheGuild.DataLayer         # Repositories (MongoDB)
│   ├── TheGuild.DataLayer.Models  # Database entities
│   └── TheGuild.Infrastructure.MongoDb  # MongoDB connection and collections
└── TheGuild.External.Discord      # Discord.Net wrapper
```

## Requirements

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [MongoDB](https://www.mongodb.com/)
- A Discord application (OAuth2 + bot token) — [Discord Developer Portal](https://discord.com/developers/applications)

## Getting started

### 1. Clone the repository

```bash
git clone https://github.com/Gordory/TheGuild.git
cd TheGuild
```

### 2. Configure

`appsettings.json` is the placeholder template kept in the repository. Real values belong in `appsettings.Development.json`, which is gitignored:

```bash
cp src/TheGuild.Api/appsettings.json src/TheGuild.Api/appsettings.Development.json
```

Fill in `appsettings.Development.json`:

```json
{
  "Discord": {
    "OAuth2": {
      "ClientId": "<your-discord-client-id>",
      "ClientSecret": "<your-discord-client-secret>"
    },
    "Bot": {
      "Token": "<your-discord-bot-token>"
    }
  },
  "MongoDB": {
    "Address": "localhost",
    "Port": 27017,
    "Database": "theguild",
    "Login": "<db-user>",
    "Password": "<db-password>"
  }
}
```

> Never commit real credentials. Every `appsettings.<Environment>.json` is gitignored; only the template is tracked.

### 3. Run

```bash
cd src/TheGuild.Api
dotnet run
```

The API starts on `https://localhost:14122` (the profile in `Properties/launchSettings.json`). Swagger UI is at `https://localhost:14122/swagger` and is enabled only in the `Development` environment.

There is no containerization yet: the repository has no `Dockerfile` or `docker-compose.yml`.

## Authentication

| Scheme | Used by | Transport | Status |
|--------|---------|-----------|--------|
| **API key** | Service clients (Discord bot) | `X-API-KEY: <guid>` header | Working |
| **Discord OAuth2** | Guild members | Cookie session | Wired up, no sign-in endpoints |

## License

[MIT](LICENSE) © 2022 Nikita Ilinykh

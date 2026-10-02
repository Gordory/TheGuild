| **External provider** | Guild members | Identity cookie session, after a Discord handshake | Working, not exercised end to end |# TheGuild

**TheGuild** is a guild management platform for World of Warcraft. The backend service exposes a REST API for member records, raid attendance, roster and guild bank. A Discord integration lets officers run the guild straight from their server, without third-party tools.

> ⚠️ **Status: early development.** The service is not usable yet. What follows describes the code as it actually stands, not where it is headed.

> The project follows a freemium model: core features are free, advanced capabilities come with a subscription.

## Current state

### Working

- **Attendance warnings** — the REST slice is live: record, read, edit and delete warnings, with every query scoped to the guild that asked.
- **Role-based permissions** — a guild defines its own named roles. Each links any number of Discord roles as its source of membership, may name individual members outright, and grants atomic permissions from a catalogue declared in code. A member holds the union of every role they match.
- **Ownership conditions** — permissions come in `…own` / `…any` pairs, and ownership of the record being touched decides which one a request needs. Reading a collection resolves to a query filter rather than to rows loaded and discarded.
- **Private comments** — withheld unless the actor holds `attendance.warning.private.read`, redacted while mapping so a new endpoint cannot forget to.
- **Accounts of our own** — ASP.NET Core Identity over MongoDB, with no password: every sign-in is an external handshake. Discord is one provider; several providers can point at one account, so a person keeps their history after switching. Accounts can be deleted, and the last remaining login cannot be removed.
- **API key authentication** — an `X-API-KEY` header carrying a GUID, looked up in MongoDB and bindable to a Discord bot (`DiscordBotApiKeyBinding`); disabled keys are rejected. A bot names the member it acts for with `X-Acting-Discord-User-Id`, and permissions resolve against that member.
- **Discord as the source of membership** — role membership is read through `Discord.Net` and cached, never mirrored into our database. We never write to Discord: channel access stays where officers already manage it.

### Not working yet

- **Role management API** — roles are read from MongoDB, but there is no endpoint to create or edit them yet, so a guild is configured by writing to the database directly.
- **The browser sign-in flow end to end** — the endpoints exist and the store behaviour underneath them is covered by tests, but the handshake has not been exercised against a real Discord application.
- **Battle.net and Google sign-in** — the model accepts more providers; none are wired yet.
- **Discord bot** — the client is registered in the API host, but there is no bot process and no slash commands.

## Roadmap

Ordered by dependency rather than by date — there are no committed timelines.

### Foundation — in progress

Everything below this section is blocked on it.

- An API for managing roles, so a guild is configured without hand-editing MongoDB
- Exercising the sign-in handshake against a real Discord application end to end
- CI, plus a `Dockerfile` and Compose setup for local runs

### Next — the core loop

- **Guild roster** — members, classes, roles. Attendance still stores a bare `DiscordUserId` with no member entity behind it
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
│   ├── TheGuild.Api               # ASP.NET Core 10 REST API
│   ├── TheGuild.Api.Models        # API-layer DTOs and models
│   ├── TheGuild.DataLayer         # Repositories (MongoDB)
│   ├── TheGuild.DataLayer.Models  # Database entities
│   └── TheGuild.Infrastructure.MongoDb  # MongoDB connection and collections
└── TheGuild.External.Discord      # Discord.Net wrapper
```

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
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
| **External provider** | Guild members | Identity cookie session, after a provider handshake | Working, not exercised end to end |

Accounts have no password. `GET /auth/{provider}/login` starts a handshake and `GET /auth/login/callback` finishes it; an unknown provider identity creates an account. A signed-in person attaches another provider with `POST /auth/link/{provider}`, lists their logins at `GET /auth/me`, detaches one with `DELETE /auth/logins/{provider}`, and deletes the account with `DELETE /auth/account`. The last remaining login cannot be detached — with no password it would lock the account forever.

If a handshake brings a **verified** address that an account already holds, the callback answers `409 link-required` and signs nobody in: linking is offered, never performed on the provider's word alone, or registering an account on someone else's address would be enough to take theirs. Proof of ownership is signing in with a provider already attached, then linking from there.

A service client also declares the member it is acting for, with `X-Acting-Discord-User-Id: <discord user id>`. Permissions are held by members, not by bots, so a request without that header resolves to no permissions at all. The bot reads the member from the Discord interaction, and the API trusts what it asserts; a malformed value fails authentication rather than silently falling back to the bot's own identity.

A refused request answers `403` with a `ProblemDetails` body whose `missingPermission` names the permission that would have covered it, so a bot can tell a member what they lack instead of just failing.

## License

[MIT](LICENSE) © 2022 Nikita Ilinykh

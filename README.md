# TheGuild

**TheGuild** — платформа для управления гильдиями в World of Warcraft. Backend-сервис предоставляет REST API для ведения записей об участниках, посещаемости рейдов, ростере и банке гильдии. Discord-интеграция позволяет управлять гильдией прямо из сервера без сторонних инструментов.

> ⚠️ **Статус: ранняя разработка.** Сервис не готов к использованию. Ниже описано фактическое состояние кода, а не целевое.

> Проект развивается по freemium-модели: базовые функции бесплатны, расширенные возможности — по подписке.

## Текущее состояние

### Работает

- **Аутентификация по API Key** — заголовок `X-API-KEY` со значением-GUID. Ключ ищется в MongoDB, поддерживается привязка к Discord-боту (`DiscordBotApiKeyBinding`); отключённые (`Enabled: false`) ключи отклоняются. Схема зарегистрирована и используется по умолчанию.
- **Доменные модели и хранилище** — `AttendanceWarning` (типы `Absence` / `Late`, публичный и приватный комментарий), `Guild` / `GuildRole` / `GuildPermissions`, репозитории MongoDB поверх общей CRUD-обвязки.
- **Расчёт прав** — `GuildPermissionsProvider` собирает разрешения участника из привязок гильдейских ролей к Discord-ролям и к конкретным пользователям, дочитывая роли участника из Discord API.
- **Обёртка над Discord.Net** — `IDiscordClientFactory` создаёт `DiscordRestClient` по bot-токену (`Discord:Bot:Token`).

### Не работает / в процессе

- **Attendance API** — `AttendanceWarningController` и регистрация `IAttendanceWarningService` закомментированы: идёт рефакторинг слоя авторизации. Публичных эндпоинтов посещаемости сейчас нет.
- **Discord OAuth2** — провайдер подключён (`AspNet.Security.OAuth.Discord` + cookie-схема), но эндпоинтов входа и callback нет, поэтому пользовательский сценарий логина недоступен.
- **Discord-бот** — проект `TheGuild.External.Discord` регистрируется через LightInject и в API-хост пока не подключён; секции `Discord:Bot` в конфигурации нет.
- **`TestController`** (`POST/GET /test`) — временные леса для ручной проверки репозитория, не часть API.

## Roadmap

Цели, к которым идёт проект:

- Discord-бот — управление гильдией через slash-команды прямо в Discord
- Рейд-менеджмент — запись на рейды, составы, логи
- Ростер гильдии — управление участниками, классами, ролями
- Банк гильдии / лут — учёт предметов и распределение лута
- Уведомления в Discord — автоматические сообщения в каналы
- Мобильное приложение — push-уведомления и управление на ходу
- Desktop-приложение — синхронизация данных
- WoW-аддоны — синхронизация данных прямо из игры
- Guild Armory — публичная страница гильдии

## Архитектура

```
TheGuild.sln
├── src/
│   ├── TheGuild.Api               # ASP.NET Core 6 REST API
│   ├── TheGuild.Api.Models        # DTO и модели API-слоя
│   ├── TheGuild.DataLayer         # Репозитории (MongoDB)
│   ├── TheGuild.DataLayer.Models  # Сущности базы данных
│   └── TheGuild.Infrastructure.MongoDb  # Подключение и коллекции MongoDB
└── TheGuild.External.Discord      # Обёртка над Discord.Net
```

## Требования

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [MongoDB](https://www.mongodb.com/)
- Discord Application (OAuth2 + Bot Token) — [Discord Developer Portal](https://discord.com/developers/applications)

## Быстрый старт

### 1. Клонировать репозиторий

```bash
git clone https://github.com/Gordory/TheGuild.git
cd TheGuild
```

### 2. Настроить конфигурацию

`appsettings.json` — шаблон с плейсхолдерами, он лежит в репозитории. Реальные значения кладутся в `appsettings.Development.json`, который игнорируется git'ом:

```bash
cp src/TheGuild.Api/appsettings.json src/TheGuild.Api/appsettings.Development.json
```

Заполни в `appsettings.Development.json`:

```json
{
  "Discord": {
    "OAuth2": {
      "ClientId": "<your-discord-client-id>",
      "ClientSecret": "<your-discord-client-secret>"
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

> Никогда не коммить реальные секреты. Файлы `appsettings.*.json`, кроме шаблона, в `.gitignore`.

### 3. Запуск

```bash
cd src/TheGuild.Api
dotnet run
```

API поднимется на `https://localhost:14122` (профиль из `Properties/launchSettings.json`). Swagger UI — `https://localhost:14122/swagger`, он включён только в окружении `Development`.

Контейнеризации пока нет: `Dockerfile` и `docker-compose.yml` в репозитории отсутствуют.

## Аутентификация

| Схема | Применение | Как передаётся | Статус |
|-------|-----------|----------------|--------|
| **API Key** | Сервисные клиенты (Discord-бот) | Заголовок `X-API-KEY: <guid>` | Работает |
| **Discord OAuth2** | Пользователи гильдии | Cookie-сессия | Подключён, эндпоинтов входа нет |

## Лицензия

[MIT](LICENSE) © 2022 Nikita Ilinykh

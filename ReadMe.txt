# Boltalka

Локальный запуск API и базы данных в Docker-контейнерах с горячей перезагрузкой.

## Требования

- [Docker](https://www.docker.com/products/docker-desktop/) (с `docker-compose`)
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (если нужно собирать проект вне контейнера)

## Быстрый старт

# 1. Клонируйте репозиторий
git clone <repo-url> boltalka
cd boltalka

# 2. (При первом запуске) Соберите образы и запустите все сервисы
docker-compose up --build

# 3. Для последующих запусков (без пересборки)
docker-compose up

http://localhost:5266/api/v1/swagger/index.html

Список запросов:

POST /api/v1/auth/register — Регистрация нового пользователя
Body:
{
  "email": "user@example.com",
  "password": "securePass123",
  "displayName": "Иван Петров"
}

POST /api/v1/auth/login — Авторизация и получение токенов
Body:
{
  "email": "user@example.com",
  "password": "securePass123"
}

POST /api/v1/auth/refresh — Обновление пары токенов (access + refresh)
Body:
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."  // строка refreshToken

POST /api/v1/auth/logout — Выход из системы (инвалидация refresh токена) [требуется авторизация]
Body:
"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."  // строка refreshToken

POST /api/v1/chats/{chatId}/calls — Начать звонок в чате [требуется авторизация]
Path: chatId (guid)
Body:
{
  "type": "audio"   // или "video"
}

POST /api/v1/calls/{callId}/accept — Принять входящий звонок [требуется авторизация]
Path: callId (guid)
Body: (отсутствует)

POST /api/v1/calls/{callId}/end — Завершить звонок [требуется авторизация]
Path: callId (guid)
Body: (отсутствует)

POST /api/v1/calls/{callId}/decline — Отклонить звонок [требуется авторизация]
Path: callId (guid)
Body: (отсутствует)

GET /api/v1/chats/{chatId}/calls/active — Получить активный звонок в чате [требуется авторизация]
Path: chatId (guid)
Query: (нет)
Body: (отсутствует)

GET /api/v1/chats/{chatId}/calls/history — История звонков в чате (пагинация) [требуется авторизация]
Path: chatId (guid)
Query: skip (int, по умолчанию 0), take (int, по умолчанию 20)
Body: (отсутствует)

POST /api/v1/chats/private — Создать личный чат [требуется авторизация]
Body:
{
  "targetUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}

POST /api/v1/chats/group — Создать групповой чат [требуется авторизация]
Body:
{
  "name": "Команда проекта",
  "memberIds": [ "id1", "id2" ]
}

GET /api/v1/chats/{chatId} — Получить информацию о чате [требуется авторизация]
Path: chatId (guid)
Body: (отсутствует)

GET /api/v1/chats/ — Получить список чатов текущего пользователя [требуется авторизация]
Query: skip (int, по умолчанию 0), take (int, по умолчанию 20)
Body: (отсутствует)

POST /api/v1/chats/{chatId}/members — Добавить участника в чат [требуется авторизация]
Path: chatId (guid)
Body:
"3fa85f64-5717-4562-b3fc-2c963f66afa6"   // Guid нового участника

DELETE /api/v1/chats/{chatId}/members/{userId} — Удалить участника из чата [требуется авторизация]
Path: chatId (guid), userId (guid)
Body: (отсутствует)

PATCH /api/v1/chats/{chatId}/members/{userId}/role — Изменить роль участника [требуется авторизация]
Path: chatId (guid), userId (guid)
Body:
"Admin"   // или "Member", "Moderator" и т.п.

PATCH /api/v1/chats/{chatId}/name — Обновить название чата [требуется авторизация]
Path: chatId (guid)
Body:
"Новое название команды"

POST /api/v1/media/upload — Загрузить файл [требуется авторизация]
Body: form-data, поле "file" (IFormFile)

GET /api/v1/media/{mediaId} — Получить информацию о медиафайле [требуется авторизация]
Path: mediaId (guid)
Body: (отсутствует)

GET /api/v1/media/{mediaId}/download — Скачать файл [требуется авторизация]
Path: mediaId (guid)
Body: (отсутствует)

DELETE /api/v1/media/{mediaId} — Удалить медиафайл [требуется авторизация]
Path: mediaId (guid)
Body: (отсутствует)

POST /api/v1/chats/{chatId}/messages — Отправить сообщение в чат [требуется авторизация]
Path: chatId (guid)
Body:
{
  "text": "Привет, коллеги!",
  "replyToMessageId": null   // или guid, если ответ на другое сообщение
}

GET /api/v1/chats/{chatId}/messages — Получить сообщения чата (пагинация + фильтр по дате) [требуется авторизация]
Path: chatId (guid)
Query: skip (int, по умолчанию 0), take (int, по умолчанию 50), before (DateTime, опционально)
Body: (отсутствует)

PUT /api/v1/messages/{messageId}/read — Отметить сообщение как прочитанное [требуется авторизация]
Path: messageId (guid)
Body: (отсутствует)

PUT /api/v1/messages/{messageId} — Редактировать текст сообщения [требуется авторизация]
Path: messageId (guid)
Body:
"Обновлённый текст сообщения"

DELETE /api/v1/messages/{messageId} — Удалить сообщение [требуется авторизация]
Path: messageId (guid)
Body: (отсутствует)

GET /api/v1/users/me — Получить профиль текущего пользователя [требуется авторизация]
Body: (отсутствует)

GET /api/v1/users/{userId} — Получить пользователя по ID [требуется авторизация]
Path: userId (guid)
Body: (отсутствует)

PUT /api/v1/users/me/display-name — Обновить отображаемое имя текущего пользователя [требуется авторизация]
Body:
"Алексей Смирнов"

PUT /api/v1/users/me/avatar — Установить аватар (по ID медиафайла) [требуется авторизация]
Body:
"3fa85f64-5717-4562-b3fc-2c963f66afa6"   // Guid загруженного медиа

GET /api/v1/users/search — Поиск пользователей по строке [доступно без авторизации]
Query: query (string, обязательно), skip (int, по умолчанию 0), take (int, по умолчанию 20)
Body: (отсутствует)

PATCH /api/v1/users/{userId}/deactivate — Деактивировать пользователя [требуется авторизация]
Path: userId (guid)
Body: (отсутствует)

PATCH /api/v1/users/{userId}/activate — Активировать пользователя [требуется авторизация]
Path: userId (guid)
Body: (отсутствует)



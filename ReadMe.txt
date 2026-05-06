# Boltalka

Локальный запуск API и базы данных в Docker-контейнерах с горячей перезагрузкой.

## Требования

- [Docker](https://www.docker.com/products/docker-desktop/) (с `docker-compose`)
- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (если нужно собирать проект вне контейнера)

## Быстрый старт

```bash
# 1. Клонируйте репозиторий
git clone <repo-url> boltalka
cd boltalka

# 2. (При первом запуске) Соберите образы и запустите все сервисы
docker-compose up --build

# 3. Для последующих запусков (без пересборки)
docker-compose up

http://localhost:5266/api/v1/swagger/index.html



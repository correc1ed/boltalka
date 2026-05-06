# Стадия разработки (hot-reload)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /src

# Копируем csproj всех проектов, сохраняя структуру папок
COPY boltalka.WebApi/boltalka.WebApi.csproj boltalka.WebApi/
COPY boltalka.Application/boltalka.Application.csproj boltalka.Application/
# добавьте остальные проекты, если есть

RUN dotnet restore boltalka.WebApi/boltalka.WebApi.csproj

# Явно указываем проект, который нужно запускать через dotnet watch
ENTRYPOINT ["dotnet", "watch", "run", "--project", "boltalka.WebApi/boltalka.WebApi.csproj", "--urls", "http://0.0.0.0:80"]

# Стадия production-сборки — без изменений
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY boltalka.WebApi/boltalka.WebApi.csproj boltalka.WebApi/
COPY boltalka.Application/boltalka.Application.csproj boltalka.Application/
RUN dotnet restore boltalka.WebApi/boltalka.WebApi.csproj
COPY . .
RUN dotnet publish boltalka.WebApi/boltalka.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "boltalka.WebApi.dll"]
# ============================================
# Стадия разработки (hot‑reload через dotnet watch)
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /src

# Сначала копируем только csproj и восстанавливаем пакеты (кэшируется)
COPY ["boltalka.WebApi.csproj", "./"]
RUN dotnet restore "boltalka.WebApi.csproj"

# Точка входа – dotnet watch с авто-пересборкой при изменениях
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://0.0.0.0:80"]

# ============================================
# Стадия production‑сборки (оставляем как есть)
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["boltalka.WebApi.csproj", "./"]
RUN dotnet restore "boltalka.WebApi.csproj"
COPY . .
RUN dotnet publish "boltalka.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Финальный production‑образ
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "boltalka.WebApi.dll"]
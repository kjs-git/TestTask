# 1. Базовый образ для запуска (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. Образ для сборки (SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем только файлы рабочих проектов
COPY ["Api/Api.csproj", "Api/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

# Восстанавливаем зависимости целевого проекта
RUN dotnet restore "Api/Api.csproj"

# Копируем весь остальной исходный код
COPY . .
WORKDIR "/src/Api"

# Собираем проект
RUN dotnet build "Api.csproj" -c Release -o /app/build

# 3. Публикуем
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Финальный этап
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
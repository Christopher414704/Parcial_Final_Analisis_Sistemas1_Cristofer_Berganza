# Dockerfile para Render - API ASP.NET Core + SQLite
# Coloca este archivo dentro de la carpeta MensajeriaApi,
# en el mismo nivel donde está MensajeriaApi.csproj.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Carpeta para guardar SQLite cuando Render tenga disco persistente.
RUN mkdir -p /data

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MensajeriaApi.csproj", "./"]
RUN dotnet restore "MensajeriaApi.csproj"

COPY . .
RUN dotnet publish "MensajeriaApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]

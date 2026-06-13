FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

RUN mkdir -p /data

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["MensajeriaApi/MensajeriaApi.csproj", "MensajeriaApi/"]
RUN dotnet restore "MensajeriaApi/MensajeriaApi.csproj"

COPY . .

WORKDIR "/src/MensajeriaApi"
RUN dotnet publish "MensajeriaApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .
COPY entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]

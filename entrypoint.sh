#!/usr/bin/env sh
set -e

echo "Iniciando MensajeriaApi en Render..."

APP_PORT="${PORT:-8080}"

export ASPNETCORE_URLS="http://0.0.0.0:${APP_PORT}"

mkdir -p /data
chmod 775 /data 2>/dev/null || true

if [ -z "$ConnectionStrings__DefaultConnection" ]; then
  export ConnectionStrings__DefaultConnection="Data Source=/data/mensajeria.db;Foreign Keys=True;"
fi

echo "Puerto configurado: ${APP_PORT}"
echo "Base SQLite configurada en: ${ConnectionStrings__DefaultConnection}"

exec dotnet MensajeriaApi.dll

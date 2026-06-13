#!/usr/bin/env sh
set -e

echo "Iniciando MensajeriaApi en Render..."

# Render usa la variable PORT para indicar en qué puerto debe escuchar el servicio.
# Si PORT no existe, usamos 8080 como valor local por defecto.
APP_PORT="${PORT:-8080}"

# ASP.NET Core debe escuchar en 0.0.0.0 para que Render pueda detectar el puerto.
export ASPNETCORE_URLS="http://0.0.0.0:${APP_PORT}"

# Carpeta de SQLite. Si Render tiene disco persistente, debe estar montado en /data.
mkdir -p /data

# Permisos preventivos. Si Render no permite chmod, no detenemos la app.
chmod 775 /data 2>/dev/null || true

# Cadena de conexión por defecto para SQLite.
# Esta variable puede sobrescribirse desde Render usando:
# ConnectionStrings__DefaultConnection
if [ -z "$ConnectionStrings__DefaultConnection" ]; then
  export ConnectionStrings__DefaultConnection="Data Source=/data/mensajeria.db;Foreign Keys=True;"
fi

echo "Puerto configurado: ${APP_PORT}"
echo "Base SQLite configurada en: ${ConnectionStrings__DefaultConnection}"
echo "Arrancando API..."

exec dotnet MensajeriaApi.dll

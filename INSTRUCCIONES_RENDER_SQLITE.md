# Archivos para desplegar MensajeriaApi en Render con SQLite

## Dónde colocar estos archivos

Coloca estos archivos dentro de la carpeta principal del proyecto API:

```text
MensajeriaApi
│
├── Dockerfile
├── entrypoint.sh
├── render.yaml
├── .dockerignore
├── appsettings.Production.json
├── MensajeriaApi.csproj
├── Program.cs
└── ...
```

El `Dockerfile` está pensado para estar en el mismo nivel que:

```text
MensajeriaApi.csproj
```

## Paquetes necesarios

Para SQLite:

```bash
dotnet remove package Microsoft.Data.SqlClient
dotnet add package Microsoft.Data.Sqlite
dotnet restore
```

Dapper y Swagger se quedan:

```bash
dotnet add package Dapper
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
```

## Render

En Render crea un Web Service con Docker.

Si tu repositorio abre directamente en la carpeta `MensajeriaApi`, usa:

```text
Dockerfile Path: ./Dockerfile
```

Si tu repositorio tiene una carpeta arriba y dentro está `MensajeriaApi`, tienes dos opciones:

Opción 1:
Configura en Render:

```text
Root Directory: MensajeriaApi
Dockerfile Path: ./Dockerfile
```

Opción 2:
Mueve estos archivos a la raíz y ajusta el Dockerfile.

## Variables de entorno recomendadas

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Data Source=/data/mensajeria.db;Foreign Keys=True;
```

## Disco persistente

Para que SQLite no se borre, monta un disco persistente en:

```text
/data
```

Si no usas disco persistente, la API puede funcionar, pero los datos pueden perderse en reinicios o redeploys.

## Qué hace entrypoint.sh

1. Crea `/data`.
2. Lee el puerto de Render con `PORT`.
3. Configura ASP.NET Core para escuchar en `0.0.0.0:$PORT`.
4. Configura SQLite en `/data/mensajeria.db`.
5. Arranca la API con `dotnet MensajeriaApi.dll`.

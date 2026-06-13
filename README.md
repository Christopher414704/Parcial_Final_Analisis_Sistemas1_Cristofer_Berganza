# MensajeriaApi - API REST de Mensajería y Paquetería

API REST desarrollada en **C# con ASP.NET Core**, orientada a un servicio de mensajería y paquetería nacional en Guatemala.

El sistema permite registrar envíos, calcular tarifas automáticamente, generar códigos de rastreo, actualizar estados, registrar historial de cambios y controlar intentos fallidos de entrega.

---

## 1. Características principales

- Registro de envíos.
- Cálculo automático de tarifa según peso.
- Generación automática de código de rastreo.
- Rastreo de paquetes por código.
- Control de estados mediante base de datos.
- Validación de transiciones permitidas.
- Registro de historial por cada cambio de estado.
- Máximo de 3 intentos de entrega.
- Cambio automático a `EnDevolucion` al tercer intento fallido.
- Descuento del 5% si remitente o destinatario tiene NIT válido.
- Pruebas unitarias con xUnit.
- Documentación y pruebas de endpoints mediante Swagger.

---

## 2. Tecnologías utilizadas

- C#
- ASP.NET Core Web API
- SQL Server
- Dapper
- Microsoft.Data.SqlClient
- Swagger / Swashbuckle
- xUnit
- .NET 8

---

## 3. Estructura del proyecto

```text
MensajeriaApi
│
├── Controllers
│   ├── EnviosController.cs
│   ├── EstadosController.cs
│   └── OficinasController.cs
│
├── Data
│   └── SqlConnectionFactory.cs
│
├── DTOs
│   ├── ActualizarEstadoRequest.cs
│   ├── ClienteRequest.cs
│   ├── CrearEnvioRequest.cs
│   ├── RegistrarIntentoRequest.cs
│   └── RastreoResponse.cs
│
├── Models
│   ├── Envio.cs
│   ├── EstadoEnvio.cs
│   ├── HistorialEnvio.cs
│   └── Oficina.cs
│
├── Rules
│   └── EnvioRules.cs
│
├── Services
│   ├── IEnvioService.cs
│   └── EnvioService.cs
│
├── appsettings.json
├── Program.cs
└── MensajeriaApi.csproj
```

Proyecto de pruebas:

```text
MensajeriaApi.Tests
│
├── DTOs
│   └── RequestTests.cs
│
├── Rules
│   ├── EnvioRulesTests.cs
│   └── EstadoRulesTests.cs
│
└── MensajeriaApi.Tests.csproj
```

---

## 4. Base de datos

La API utiliza una base de datos SQL Server llamada:

```text
MensajeriaDB
```

Antes de ejecutar el proyecto, se debe crear la base de datos ejecutando el script SQL correspondiente en SQL Server Management Studio.

Tablas principales:

```text
Cliente
Oficina
Envio
HistorialEnvio
EstadoEnvio
TransicionEstado
```

Vista principal:

```text
vw_RastreoEnvios
```

---

## 5. Configuración de conexión

En el archivo:

```text
appsettings.json
```

configurar la conexión a SQL Server.

Ejemplo usando SQL Server Express:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=MensajeriaDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Ejemplo usando instancia local por defecto:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MensajeriaDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Ejemplo usando LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MensajeriaDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

---

## 6. Instalación de paquetes

Desde la carpeta del proyecto principal:

```bash
dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
```

Para restaurar paquetes:

```bash
dotnet restore
```

---

## 7. Ejecutar la API

Desde la carpeta del proyecto:

```bash
cd MensajeriaApi
dotnet run
```

La consola mostrará una URL parecida a:

```text
https://localhost:7182
```

Luego abrir Swagger en el navegador:

```text
https://localhost:7182/swagger
```

Si el puerto cambia, usar el puerto que aparece en la terminal.

---

## 8. Endpoints disponibles

### Oficinas

Listar oficinas:

```http
GET /api/oficinas
```

---

### Estados

Listar todos los estados:

```http
GET /api/estados
```

Listar estados siguientes permitidos:

```http
GET /api/estados/{idEstadoActual}/siguientes
```

Ejemplo:

```http
GET /api/estados/1/siguientes
```

---

### Envíos

Listar envíos:

```http
GET /api/envios
```

Buscar envío por código de rastreo:

```http
GET /api/envios/{codigoRastreo}
```

Registrar envío:

```http
POST /api/envios
```

Actualizar estado:

```http
PUT /api/envios/{idEnvio}/estado
```

Registrar intento fallido de entrega:

```http
POST /api/envios/{idEnvio}/intento-entrega
```

---

## 9. Estados del envío

Los estados se manejan mediante IDs en la base de datos.

```text
1 = Registrado
2 = EnTransito
3 = EnReparto
4 = Entregado
5 = EnDevolucion
6 = Devuelto
```

Transiciones permitidas:

```text
Registrado → EnTransito
EnTransito → EnReparto
EnReparto → Entregado
EnReparto → EnDevolucion
EnDevolucion → Devuelto
```

No se permite regresar a estados anteriores porque la API valida la transición contra la tabla:

```text
TransicionEstado
```

---

## 10. Reglas de tarifa

La tarifa se calcula automáticamente según el peso del paquete:

```text
Peso <= 1kg          = Q25.00
1.01kg a 5kg         = Q45.00
5.01kg a 10kg        = Q75.00
Mayor a 10kg         = Q100.00
```

Si el remitente o destinatario tiene NIT válido, se aplica:

```text
5% de descuento
```

---

## 11. Probar en Swagger

### 11.1. Registrar envío

Endpoint:

```http
POST /api/envios
```

Body:

```json
{
  "remitente": {
    "nombre": "Juan Perez",
    "telefono": "5555-1111",
    "direccion": "Zona 1, Ciudad de Guatemala",
    "departamento": "Guatemala",
    "nit": "CF"
  },
  "destinatario": {
    "nombre": "Maria Lopez",
    "telefono": "5555-2222",
    "direccion": "Barrio El Centro, Jalapa",
    "departamento": "Jalapa",
    "nit": "CF"
  },
  "idOficinaOrigen": 1,
  "idOficinaDestino": 2,
  "pesoKg": 4.5,
  "notas": "Paquete registrado desde Swagger"
}
```

Respuesta esperada:

```json
{
  "idEnvio": 1,
  "codigoRastreo": "ENV-20260613-0001",
  "idEstado": 1,
  "estado": "Registrado",
  "pesoKg": 4.5,
  "tarifaBase": 45,
  "descuento": 0,
  "totalPagar": 45,
  "intentosEntrega": 0
}
```

Guardar estos datos para las siguientes pruebas:

```text
idEnvio
codigoRastreo
```

---

### 11.2. Cambiar de Registrado a EnTransito

Endpoint:

```http
PUT /api/envios/{idEnvio}/estado
```

Body:

```json
{
  "idEstadoNuevo": 2,
  "idOficina": 1,
  "notas": "El paquete salió de la oficina origen"
}
```

---

### 11.3. Cambiar de EnTransito a EnReparto

Endpoint:

```http
PUT /api/envios/{idEnvio}/estado
```

Body:

```json
{
  "idEstadoNuevo": 3,
  "idOficina": 2,
  "notas": "El paquete llegó a la oficina destino y salió a reparto"
}
```

---

### 11.4. Cambiar de EnReparto a Entregado

Endpoint:

```http
PUT /api/envios/{idEnvio}/estado
```

Body:

```json
{
  "idEstadoNuevo": 4,
  "idOficina": 2,
  "notas": "Paquete entregado correctamente"
}
```

---

### 11.5. Registrar intento fallido

Solo funciona cuando el envío está en estado:

```text
EnReparto
```

Endpoint:

```http
POST /api/envios/{idEnvio}/intento-entrega
```

Body:

```json
{
  "idOficina": 2,
  "notas": "Cliente no se encontraba en la dirección"
}
```

Al tercer intento fallido, el envío cambia automáticamente a:

```text
EnDevolucion
```

---

### 11.6. Cambiar de EnDevolucion a Devuelto

Endpoint:

```http
PUT /api/envios/{idEnvio}/estado
```

Body:

```json
{
  "idEstadoNuevo": 6,
  "idOficina": 1,
  "notas": "Paquete devuelto a la oficina origen"
}
```

---

### 11.7. Buscar por código de rastreo

Endpoint:

```http
GET /api/envios/{codigoRastreo}
```

Ejemplo:

```http
GET /api/envios/ENV-20260613-0001
```

La respuesta debe incluir:

```text
Datos del envío
Estado actual
Intentos de entrega
Historial de cambios
Oficina donde se actualizó cada estado
Fecha automática de cada cambio
```

---

## 12. Prueba de error: estado inválido

Si un envío está en `Registrado`, no debería poder pasar directamente a `Entregado`.

Endpoint:

```http
PUT /api/envios/{idEnvio}/estado
```

Body incorrecto:

```json
{
  "idEstadoNuevo": 4,
  "idOficina": 1,
  "notas": "Intento inválido de pasar directo a Entregado"
}
```

Respuesta esperada:

```json
{
  "mensaje": "No se puede cambiar de Registrado a Entregado."
}
```

---

## 13. Proyecto de pruebas unitarias

El proyecto de pruebas utiliza xUnit.

Librerías necesarias:

```bash
dotnet add MensajeriaApi.Tests package Microsoft.NET.Test.Sdk
dotnet add MensajeriaApi.Tests package xunit
dotnet add MensajeriaApi.Tests package xunit.runner.visualstudio
dotnet add MensajeriaApi.Tests package coverlet.collector
```

El proyecto de pruebas debe tener referencia al proyecto principal:

```bash
dotnet add MensajeriaApi.Tests/MensajeriaApi.Tests.csproj reference MensajeriaApi/MensajeriaApi.csproj
```

---

## 14. Ejecutar pruebas unitarias

Desde la carpeta donde está la solución:

```bash
dotnet test
```

O directamente:

```bash
dotnet test MensajeriaApi.Tests
```

Resultado esperado:

```text
Passed! - Failed: 0, Passed: X, Skipped: 0
```

---

## 15. Qué validan las pruebas

Las pruebas unitarias validan:

```text
Tarifa para peso <= 1kg
Tarifa para peso entre 1.01kg y 5kg
Tarifa para peso entre 5.01kg y 10kg
Tarifa para peso mayor a 10kg
Error cuando el peso es inválido
Validación básica de NIT
Cálculo de descuento
Cálculo de total a pagar
Intentos de entrega
Cambio a devolución al tercer intento
Estados finales
DTOs principales
```

---

## 16. Comandos útiles

Limpiar proyecto:

```bash
dotnet clean
```

Restaurar paquetes:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

Ejecutar API:

```bash
dotnet run --project MensajeriaApi
```

Ejecutar pruebas:

```bash
dotnet test
```

---

## 17. Posibles errores comunes

### Error de conexión a SQL Server

Si aparece un error como:

```text
Error Locating Server/Instance Specified
```

verificar:

```text
1. Que SQL Server esté encendido.
2. Que el nombre de la instancia sea correcto.
3. Que la base de datos MensajeriaDB exista.
4. Que la cadena de conexión en appsettings.json sea correcta.
```

### Error en Swagger con Microsoft.OpenApi.Models

Si aparece un error relacionado con:

```text
Microsoft.OpenApi.Models
OpenApiInfo
```

usar una versión estable de Swashbuckle:

```bash
dotnet remove package Swashbuckle.AspNetCore
dotnet add package Swashbuckle.AspNetCore --version 6.6.2
dotnet restore
```

---

## 18. Orden recomendado para probar el flujo completo

```text
1. Ejecutar la API.
2. Abrir Swagger.
3. GET /api/oficinas.
4. GET /api/estados.
5. POST /api/envios.
6. Copiar idEnvio y codigoRastreo.
7. PUT /api/envios/{idEnvio}/estado con idEstadoNuevo = 2.
8. PUT /api/envios/{idEnvio}/estado con idEstadoNuevo = 3.
9. POST /api/envios/{idEnvio}/intento-entrega tres veces.
10. Verificar que cambió a EnDevolucion.
11. PUT /api/envios/{idEnvio}/estado con idEstadoNuevo = 6.
12. GET /api/envios/{codigoRastreo}.
13. Revisar historial completo.
14. Ejecutar dotnet test.
```

---

## 19. Autor

Proyecto académico desarrollado para practicar:

```text
API REST
Reglas de negocio
SQL Server
Validación de estados
Swagger
Pruebas unitarias
```

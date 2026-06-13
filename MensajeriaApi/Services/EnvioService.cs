using Dapper;
using MensajeriaApi.Data;
using MensajeriaApi.DTOs;
using MensajeriaApi.Models;
using Microsoft.Data.SqlClient;
using MensajeriaApi.Rules;

namespace MensajeriaApi.Services;

public class EnvioService : IEnvioService
{
    private readonly SqlConnectionFactory _connectionFactory;

    private const int EstadoRegistrado = 1;
    private const int EstadoEnReparto = 3;
    private const int EstadoEnDevolucion = 5;

    public EnvioService(SqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Oficina>> ListarOficinasAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Oficina>(
            "SELECT * FROM Oficina WHERE Activa = 1 ORDER BY Departamento");
    }

    public async Task<IEnumerable<EstadoEnvio>> ListarEstadosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<EstadoEnvio>(
            """
            SELECT IdEstado, NombreEstado, EsFinal
            FROM EstadoEnvio
            ORDER BY IdEstado
            """);
    }

    public async Task<IEnumerable<EstadoEnvio>> ListarEstadosSiguientesAsync(int idEstadoActual)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<EstadoEnvio>(
            """
            SELECT 
                e.IdEstado,
                e.NombreEstado,
                e.EsFinal
            FROM TransicionEstado t
            INNER JOIN EstadoEnvio e ON t.IdEstadoDestino = e.IdEstado
            WHERE t.IdEstadoOrigen = @idEstadoActual
            ORDER BY e.IdEstado
            """,
            new { idEstadoActual });
    }

    public async Task<IEnumerable<RastreoResponse>> ListarEnviosAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        var envios = (await connection.QueryAsync<RastreoResponse>(
            "SELECT * FROM vw_RastreoEnvios ORDER BY FechaRegistro DESC")).ToList();

        foreach (var envio in envios)
        {
            envio.Historial = (await ObtenerHistorialAsync(envio.IdEnvio)).ToList();
        }

        return envios;
    }

    public async Task<RastreoResponse?> ObtenerPorCodigoAsync(string codigo)
    {
        using var connection = _connectionFactory.CreateConnection();

        var envio = await connection.QueryFirstOrDefaultAsync<RastreoResponse>(
            """
            SELECT *
            FROM vw_RastreoEnvios
            WHERE CodigoRastreo = @codigo
            """,
            new { codigo });

        if (envio == null)
            return null;

        envio.Historial = (await ObtenerHistorialAsync(envio.IdEnvio)).ToList();

        return envio;
    }

    public async Task<RastreoResponse> CrearEnvioAsync(CrearEnvioRequest request)
    {
        if (request.PesoKg <= 0)
            throw new InvalidOperationException("El peso debe ser mayor a 0.");

        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            bool nitRemitenteValido = EnvioRules.NitEsValido(request.Remitente.NIT);
            bool nitDestinatarioValido = EnvioRules.NitEsValido(request.Destinatario.NIT);

            int idRemitente = await InsertarClienteAsync(
                connection,
                transaction,
                request.Remitente,
                nitRemitenteValido);

            int idDestinatario = await InsertarClienteAsync(
                connection,
                transaction,
                request.Destinatario,
                nitDestinatarioValido);

            decimal tarifaBase = EnvioRules.CalcularTarifa(request.PesoKg);

            decimal descuento = EnvioRules.CalcularDescuento(
                    tarifaBase,
                    nitRemitenteValido,
                    nitDestinatarioValido);

                decimal totalPagar = EnvioRules.CalcularTotal(tarifaBase, descuento);

            string codigoRastreo = await GenerarCodigoRastreoAsync(connection, transaction);

            string nombreEstadoRegistrado = await ObtenerNombreEstadoAsync(
                connection,
                transaction,
                EstadoRegistrado) ?? "Registrado";

            int idEnvio = await connection.QuerySingleAsync<int>(
                """
                INSERT INTO Envio
                (
                    CodigoRastreo,
                    IdRemitente,
                    IdDestinatario,
                    IdOficinaOrigen,
                    IdOficinaDestino,
                    PesoKg,
                    TarifaBase,
                    Descuento,
                    TotalPagar,
                    IdEstado,
                    Estado
                )
                VALUES
                (
                    @CodigoRastreo,
                    @IdRemitente,
                    @IdDestinatario,
                    @IdOficinaOrigen,
                    @IdOficinaDestino,
                    @PesoKg,
                    @TarifaBase,
                    @Descuento,
                    @TotalPagar,
                    @IdEstado,
                    @Estado
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);
                """,
                new
                {
                    CodigoRastreo = codigoRastreo,
                    IdRemitente = idRemitente,
                    IdDestinatario = idDestinatario,
                    request.IdOficinaOrigen,
                    request.IdOficinaDestino,
                    request.PesoKg,
                    TarifaBase = tarifaBase,
                    Descuento = descuento,
                    TotalPagar = totalPagar,
                    IdEstado = EstadoRegistrado,
                    Estado = nombreEstadoRegistrado
                },
                transaction);

            await InsertarHistorialAsync(
                connection,
                transaction,
                idEnvio,
                EstadoRegistrado,
                nombreEstadoRegistrado,
                request.IdOficinaOrigen,
                request.Notas ?? "Envío registrado correctamente.");

            transaction.Commit();

            var envioCreado = await ObtenerPorCodigoAsync(codigoRastreo);

            return envioCreado!;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task ActualizarEstadoAsync(int idEnvio, ActualizarEstadoRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            var envio = await connection.QueryFirstOrDefaultAsync<Envio>(
                """
                SELECT *
                FROM Envio
                WHERE IdEnvio = @idEnvio
                """,
                new { idEnvio },
                transaction);

            if (envio == null)
                throw new InvalidOperationException("El envío no existe.");

            string estadoActual = await ObtenerNombreEstadoAsync(
                connection,
                transaction,
                envio.IdEstado) ?? envio.Estado;

            string? estadoNuevo = await ObtenerNombreEstadoAsync(
                connection,
                transaction,
                request.IdEstadoNuevo);

            if (estadoNuevo == null)
                throw new InvalidOperationException("El estado solicitado no existe.");

            if (envio.IdEstado == request.IdEstadoNuevo)
                throw new InvalidOperationException($"El envío ya se encuentra en estado {estadoActual}.");

            bool permitido = await TransicionPermitidaAsync(
                connection,
                transaction,
                envio.IdEstado,
                request.IdEstadoNuevo);

            if (!permitido)
                throw new InvalidOperationException($"No se puede cambiar de {estadoActual} a {estadoNuevo}.");

            await connection.ExecuteAsync(
                """
                UPDATE Envio
                SET IdEstado = @IdEstadoNuevo,
                    Estado = @EstadoNuevo,
                    FechaActualizacion = SYSDATETIME()
                WHERE IdEnvio = @IdEnvio
                """,
                new
                {
                    IdEstadoNuevo = request.IdEstadoNuevo,
                    EstadoNuevo = estadoNuevo,
                    IdEnvio = idEnvio
                },
                transaction);

            await InsertarHistorialAsync(
                connection,
                transaction,
                idEnvio,
                request.IdEstadoNuevo,
                estadoNuevo,
                request.IdOficina,
                request.Notas);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task RegistrarIntentoEntregaAsync(int idEnvio, RegistrarIntentoRequest request)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            var envio = await connection.QueryFirstOrDefaultAsync<Envio>(
                """
                SELECT *
                FROM Envio
                WHERE IdEnvio = @idEnvio
                """,
                new { idEnvio },
                transaction);

            if (envio == null)
                throw new InvalidOperationException("El envío no existe.");

            string estadoActual = await ObtenerNombreEstadoAsync(
                connection,
                transaction,
                envio.IdEstado) ?? envio.Estado;

            if (envio.IdEstado != EstadoEnReparto)
                throw new InvalidOperationException("Solo se pueden registrar intentos cuando el envío está EnReparto.");

            if (envio.IntentosEntrega >= 3)
                throw new InvalidOperationException("El envío ya alcanzó el máximo de 3 intentos.");

            int nuevoIntento = EnvioRules.CalcularNuevoIntento(envio.IntentosEntrega);

            int nuevoIdEstado = EnvioRules.DebePasarADevolucion(nuevoIntento)
                ? EstadoEnDevolucion
                : EstadoEnReparto;

            string nuevoNombreEstado = await ObtenerNombreEstadoAsync(
                connection,
                transaction,
                nuevoIdEstado) ?? estadoActual;

            if (nuevoIdEstado != envio.IdEstado)
            {
                bool permitido = await TransicionPermitidaAsync(
                    connection,
                    transaction,
                    envio.IdEstado,
                    nuevoIdEstado);

                if (!permitido)
                    throw new InvalidOperationException($"No se puede cambiar de {estadoActual} a {nuevoNombreEstado}.");
            }

            await connection.ExecuteAsync(
                """
                UPDATE Envio
                SET IntentosEntrega = @Intentos,
                    IdEstado = @IdEstado,
                    Estado = @Estado,
                    FechaActualizacion = SYSDATETIME()
                WHERE IdEnvio = @IdEnvio
                """,
                new
                {
                    Intentos = nuevoIntento,
                    IdEstado = nuevoIdEstado,
                    Estado = nuevoNombreEstado,
                    IdEnvio = idEnvio
                },
                transaction);

            string nota = request.Notas ?? $"Intento de entrega fallido {nuevoIntento}/3.";

            if (nuevoIntento >= 3)
            {
                nota += " El envío cambió automáticamente a EnDevolucion.";
            }

            await InsertarHistorialAsync(
                connection,
                transaction,
                idEnvio,
                nuevoIdEstado,
                nuevoNombreEstado,
                request.IdOficina,
                nota);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private async Task<int> InsertarClienteAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        ClienteRequest cliente,
        bool nitValido)
    {
        return await connection.QuerySingleAsync<int>(
            """
            INSERT INTO Cliente
            (
                Nombre,
                Telefono,
                Direccion,
                Departamento,
                NIT,
                NitValido
            )
            VALUES
            (
                @Nombre,
                @Telefono,
                @Direccion,
                @Departamento,
                @NIT,
                @NitValido
            );

            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            new
            {
                cliente.Nombre,
                cliente.Telefono,
                cliente.Direccion,
                cliente.Departamento,
                cliente.NIT,
                NitValido = nitValido
            },
            transaction);
    }

    private async Task<string> GenerarCodigoRastreoAsync(
        SqlConnection connection,
        SqlTransaction transaction)
    {
        string fecha = DateTime.Now.ToString("yyyyMMdd");
        string prefijo = $"ENV-{fecha}-";

        int correlativo = await connection.QuerySingleAsync<int>(
            """
            SELECT COUNT(*) + 1
            FROM Envio
            WHERE CodigoRastreo LIKE @Prefijo + '%'
            """,
            new { Prefijo = prefijo },
            transaction);

        return $"{prefijo}{correlativo:0000}";
    }

    private async Task InsertarHistorialAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int idEnvio,
        int idEstadoNuevo,
        string estadoNuevo,
        int idOficina,
        string? notas)
    {
        await connection.ExecuteAsync(
            """
            INSERT INTO HistorialEnvio
            (
                IdEnvio,
                IdEstadoNuevo,
                EstadoNuevo,
                IdOficina,
                Notas
            )
            VALUES
            (
                @IdEnvio,
                @IdEstadoNuevo,
                @EstadoNuevo,
                @IdOficina,
                @Notas
            )
            """,
            new
            {
                IdEnvio = idEnvio,
                IdEstadoNuevo = idEstadoNuevo,
                EstadoNuevo = estadoNuevo,
                IdOficina = idOficina,
                Notas = notas
            },
            transaction);
    }

    private async Task<IEnumerable<HistorialEnvio>> ObtenerHistorialAsync(int idEnvio)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<HistorialEnvio>(
            """
            SELECT 
                h.IdHistorial,
                h.IdEnvio,
                h.IdEstadoNuevo,
                es.NombreEstado AS EstadoNuevo,
                h.IdOficina,
                o.Nombre AS Oficina,
                h.FechaCambio,
                h.Notas
            FROM HistorialEnvio h
            INNER JOIN EstadoEnvio es ON h.IdEstadoNuevo = es.IdEstado
            INNER JOIN Oficina o ON h.IdOficina = o.IdOficina
            WHERE h.IdEnvio = @idEnvio
            ORDER BY h.FechaCambio ASC
            """,
            new { idEnvio });
    }

    private async Task<string?> ObtenerNombreEstadoAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int idEstado)
    {
        return await connection.QueryFirstOrDefaultAsync<string>(
            """
            SELECT NombreEstado
            FROM EstadoEnvio
            WHERE IdEstado = @idEstado
            """,
            new { idEstado },
            transaction);
    }

    private async Task<bool> TransicionPermitidaAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int idEstadoActual,
        int idEstadoNuevo)
    {
        int existe = await connection.QuerySingleAsync<int>(
            """
            SELECT COUNT(*)
            FROM TransicionEstado
            WHERE IdEstadoOrigen = @idEstadoActual
              AND IdEstadoDestino = @idEstadoNuevo
            """,
            new
            {
                idEstadoActual,
                idEstadoNuevo
            },
            transaction);

        return existe > 0;
    }

    private decimal CalcularTarifa(decimal pesoKg)
    {
        if (pesoKg <= 1)
            return 25.00m;

        if (pesoKg <= 5)
            return 45.00m;

        if (pesoKg <= 10)
            return 75.00m;

        return 100.00m;
    }

    private bool NitEsValido(string? nit)
    {
        if (string.IsNullOrWhiteSpace(nit))
            return false;

        nit = nit.Trim().Replace("-", "").Replace(" ", "").ToUpper();

        if (nit == "CF")
            return false;

        if (nit.Length < 2)
            return false;

        string numero = nit[..^1];
        char digitoVerificador = nit[^1];

        if (!numero.All(char.IsDigit))
            return false;

        int factor = numero.Length + 1;
        int suma = 0;

        foreach (char c in numero)
        {
            suma += int.Parse(c.ToString()) * factor;
            factor--;
        }

        int residuo = suma % 11;
        int resultado = 11 - residuo;

        char esperado = resultado switch
        {
            10 => 'K',
            11 => '0',
            _ => resultado.ToString()[0]
        };

        return digitoVerificador == esperado;
    }
}
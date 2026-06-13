namespace MensajeriaApi.Rules;

public static class EnvioRules
{
    public static decimal CalcularTarifa(decimal pesoKg)
    {
        if (pesoKg <= 0)
            throw new InvalidOperationException("El peso debe ser mayor a 0.");

        if (pesoKg <= 1)
            return 25.00m;

        if (pesoKg <= 5)
            return 45.00m;

        if (pesoKg <= 10)
            return 75.00m;

        return 100.00m;
    }

    public static bool NitEsValido(string? nit)
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

    public static decimal CalcularDescuento(decimal tarifaBase, bool nitRemitenteValido, bool nitDestinatarioValido)
    {
        if (tarifaBase < 0)
            throw new InvalidOperationException("La tarifa no puede ser negativa.");

        if (nitRemitenteValido || nitDestinatarioValido)
            return Math.Round(tarifaBase * 0.05m, 2);

        return 0;
    }

    public static decimal CalcularTotal(decimal tarifaBase, decimal descuento)
    {
        if (tarifaBase < 0)
            throw new InvalidOperationException("La tarifa no puede ser negativa.");

        if (descuento < 0)
            throw new InvalidOperationException("El descuento no puede ser negativo.");

        return tarifaBase - descuento;
    }

    public static int CalcularNuevoIntento(int intentosActuales)
    {
        if (intentosActuales < 0)
            throw new InvalidOperationException("Los intentos no pueden ser negativos.");

        if (intentosActuales >= 3)
            throw new InvalidOperationException("El envío ya alcanzó el máximo de 3 intentos.");

        return intentosActuales + 1;
    }

    public static bool DebePasarADevolucion(int nuevoIntento)
    {
        return nuevoIntento >= 3;
    }
}
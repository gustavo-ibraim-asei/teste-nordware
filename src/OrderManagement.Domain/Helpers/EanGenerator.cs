namespace OrderManagement.Domain.Helpers;

/// <summary>
/// Helper para gerar códigos de barras no formato EAN (European Article Number)
/// </summary>
public static class EanGenerator
{
    /// <summary>
    /// Gera um código EAN-13 válido baseado em um número base
    /// EAN-13: 13 dígitos, sendo o último o dígito verificador
    /// </summary>
    public static string GenerateEan13(long baseNumber)
    {
        // Garantir que o número base tenha no máximo 12 dígitos
        string baseString = baseNumber.ToString();
        if (baseString.Length > 12)
        {
            baseString = baseString.Substring(0, 12);
        }
        else if (baseString.Length < 12)
        {
            // Preencher com zeros à esquerda
            baseString = baseString.PadLeft(12, '0');
        }

        // Calcular dígito verificador
        int checkDigit = CalculateEanCheckDigit(baseString);
        
        return baseString + checkDigit.ToString();
    }

    /// <summary>
    /// Gera um código EAN-8 válido baseado em um número base
    /// EAN-8: 8 dígitos, sendo o último o dígito verificador
    /// </summary>
    public static string GenerateEan8(long baseNumber)
    {
        // Garantir que o número base tenha no máximo 7 dígitos
        string baseString = baseNumber.ToString();
        if (baseString.Length > 7)
        {
            baseString = baseString.Substring(0, 7);
        }
        else if (baseString.Length < 7)
        {
            // Preencher com zeros à esquerda
            baseString = baseString.PadLeft(7, '0');
        }

        // Calcular dígito verificador
        int checkDigit = CalculateEanCheckDigit(baseString);
        
        return baseString + checkDigit.ToString();
    }

    /// <summary>
    /// Gera EAN-13 a partir de uma string de códigos (ProductCode, ColorCode, SizeCode)
    /// </summary>
    public static string GenerateEan13FromCodes(string productCode, string colorCode, string sizeCode)
    {
        // Combinar códigos e converter para número
        string combined = $"{productCode}{colorCode}{sizeCode}";
        
        // Remover caracteres não numéricos e pegar apenas números
        string numericOnly = new string(combined.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrEmpty(numericOnly))
        {
            // Se não houver números, usar hash dos códigos
            int hash = Math.Abs(combined.GetHashCode());
            numericOnly = hash.ToString();
        }

        // Limitar a 12 dígitos para EAN-13
        if (numericOnly.Length > 12)
        {
            numericOnly = numericOnly.Substring(0, 12);
        }
        else if (numericOnly.Length < 12)
        {
            numericOnly = numericOnly.PadLeft(12, '0');
        }

        // Calcular dígito verificador
        int checkDigit = CalculateEanCheckDigit(numericOnly);
        
        return numericOnly + checkDigit.ToString();
    }

    /// <summary>
    /// Calcula o dígito verificador para EAN (algoritmo padrão)
    /// </summary>
    private static int CalculateEanCheckDigit(string baseNumber)
    {
        int sum = 0;
        
        for (int i = 0; i < baseNumber.Length; i++)
        {
            int digit = int.Parse(baseNumber[i].ToString());
            
            // Posições ímpares (1, 3, 5...) multiplicam por 1
            // Posições pares (2, 4, 6...) multiplicam por 3
            if (i % 2 == 0)
            {
                sum += digit;
            }
            else
            {
                sum += digit * 3;
            }
        }
        
        // O dígito verificador é o número que, somado ao resultado, dá múltiplo de 10
        int remainder = sum % 10;
        return remainder == 0 ? 0 : 10 - remainder;
    }

    /// <summary>
    /// Valida se um código EAN é válido
    /// </summary>
    public static bool IsValidEan(string ean)
    {
        if (string.IsNullOrWhiteSpace(ean))
            return false;

        // EAN deve ter 8 ou 13 dígitos
        if (ean.Length != 8 && ean.Length != 13)
            return false;

        // Deve conter apenas dígitos
        if (!ean.All(char.IsDigit))
            return false;

        // Verificar dígito verificador
        string baseNumber = ean.Substring(0, ean.Length - 1);
        int providedCheckDigit = int.Parse(ean[ean.Length - 1].ToString());
        int calculatedCheckDigit = CalculateEanCheckDigit(baseNumber);

        return providedCheckDigit == calculatedCheckDigit;
    }
}




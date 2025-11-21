using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace ETLWorkerService.Infrastructure.Repositories
{
    /// <summary>
    /// Converter personalizado para convertir IDs con prefijo (C13569, P16834) a int
    /// </summary>
    public class PrefixedIntConverter : Int32Converter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return base.ConvertFromString(text, row, memberMapData);

            // Extraer solo los dígitos (ej: "C13569" -> "13569")
            var digits = new string(text.Where(char.IsDigit).ToArray());
            
            if (string.IsNullOrEmpty(digits))
                return 0;

            return base.ConvertFromString(digits, row, memberMapData);
        }
    }
}

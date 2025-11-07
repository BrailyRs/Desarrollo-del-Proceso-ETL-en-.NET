using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;

namespace ETLWorkerService.Core.TypeConverters
{
    public class PrefixedIntConverter : DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            // Remove 'C' or 'P' prefix if present
            if (text.StartsWith("C", StringComparison.OrdinalIgnoreCase) || text.StartsWith("P", StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(1);
            }

            if (int.TryParse(text, out int result))
            {
                return result;
            }

            return base.ConvertFromString(text, row, memberMapData);
        }
    }
}
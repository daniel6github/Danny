using System;
namespace Danny.Core.Csv;

using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

public class CsvMethod
{
    public static void WriteCsv<T>(string fileName, IList<T> records) where T : new()
    {
        using var writer = new StreamWriter(fileName);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);
    }

    public static IList<T> ReadCsv<T>(string fileName, CsvConfiguration config) where T : class
    {
        using var reader = new StreamReader(fileName);
        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }
}
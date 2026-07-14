using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using ClosedXML.Excel;

namespace CleanApi.Infrastructure.Services;

/// <summary>ClosedXML (MIT-licensed) implementation of <see cref="IExcelGenerator"/>.</summary>
public sealed class ExcelGenerator : IExcelGenerator
{
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public FileDto Export(
        string sheetName,
        string fileName,
        IReadOnlyDictionary<string, string> headers,
        IEnumerable<IReadOnlyDictionary<string, object?>> rows)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        var keys = headers.Keys.ToList();

        for (var col = 0; col < keys.Count; col++)
        {
            var cell = worksheet.Cell(1, col + 1);
            cell.Value = headers[keys[col]];
            cell.Style.Font.Bold = true;
        }

        var rowIndex = 2;
        foreach (var row in rows)
        {
            for (var col = 0; col < keys.Count; col++)
            {
                row.TryGetValue(keys[col], out var value);
                worksheet.Cell(rowIndex, col + 1).Value = ToCellValue(value);
            }

            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new FileDto(stream.ToArray(), fileName, XlsxContentType);
    }

    public IReadOnlyList<IReadOnlyDictionary<string, string>> Read(Stream xlsxStream)
    {
        using var workbook = new XLWorkbook(xlsxStream);
        var worksheet = workbook.Worksheet(1);

        var rowsUsed = worksheet.RowsUsed().ToList();
        if (rowsUsed.Count == 0)
        {
            return [];
        }

        var headerCells = rowsUsed[0].CellsUsed().ToList();
        var headers = headerCells.Select(c => c.GetString()).ToList();

        var result = new List<IReadOnlyDictionary<string, string>>();
        foreach (var row in rowsUsed.Skip(1))
        {
            var dict = new Dictionary<string, string>();
            for (var i = 0; i < headers.Count; i++)
            {
                dict[headers[i]] = row.Cell(i + 1).GetString();
            }

            result.Add(dict);
        }

        return result;
    }

    private static XLCellValue ToCellValue(object? value) => value switch
    {
        null => string.Empty,
        string s => s,
        bool b => b,
        int i => i,
        long l => l,
        double d => d,
        float f => f,
        decimal m => m,
        DateTime dt => dt,
        DateTimeOffset dto => dto.DateTime,
        _ => value.ToString() ?? string.Empty,
    };
}

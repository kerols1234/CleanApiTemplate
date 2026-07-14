using CleanApi.Application.Common.Models;

namespace CleanApi.Application.Common.Interfaces;

/// <summary>
/// Generates and reads Excel workbooks (implemented with ClosedXML in Infrastructure — MIT-licensed,
/// unlike EPPlus which is commercial). Column order follows the header dictionary insertion order.
/// </summary>
public interface IExcelGenerator
{
    /// <summary>Writes <paramref name="rows"/> to a single-sheet .xlsx. <paramref name="headers"/> maps
    /// column key → display header; each row maps column key → cell value.</summary>
    FileDto Export(
        string sheetName,
        string fileName,
        IReadOnlyDictionary<string, string> headers,
        IEnumerable<IReadOnlyDictionary<string, object?>> rows);

    /// <summary>Reads the first worksheet into a list of rows (header key → string cell value).</summary>
    IReadOnlyList<IReadOnlyDictionary<string, string>> Read(Stream xlsxStream);
}

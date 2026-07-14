using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CleanApi.Infrastructure.Pdf;

/// <summary>
/// QuestPDF implementation of <see cref="IPdfGenerator"/>. Requires the Community license to be set
/// once at startup (see Program.cs): <c>QuestPDF.Settings.License = LicenseType.Community;</c>.
/// </summary>
public sealed class QuestPdfGenerator : IPdfGenerator
{
    public FileDto GenerateProductCatalog(ProductCatalogPdfModel model)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text(model.Title)
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content().PaddingVertical(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Name");
                        header.Cell().Element(HeaderCell).Text("SKU");
                        header.Cell().Element(HeaderCell).Text("Price");
                        header.Cell().Element(HeaderCell).Text("Stock");

                        static IContainer HeaderCell(IContainer c) =>
                            c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                    });

                    foreach (var row in model.Rows)
                    {
                        table.Cell().Element(BodyCell).Text(row.Name);
                        table.Cell().Element(BodyCell).Text(row.Sku);
                        table.Cell().Element(BodyCell).Text($"{row.Price:0.00} {row.Currency}");
                        table.Cell().Element(BodyCell).Text(row.Stock.ToString());
                    }

                    static IContainer BodyCell(IContainer c) =>
                        c.PaddingVertical(3).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        });

        return new FileDto(document.GeneratePdf(), "product-catalog.pdf", "application/pdf");
    }
}

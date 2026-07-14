using Asp.Versioning;
using CleanApi.Api.Authorization;
using CleanApi.Api.Common;
using CleanApi.Application.Modules.Products.Commands;
using CleanApi.Application.Modules.Products.Queries;
using CleanApi.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApi.Api.Controllers;

[ApiVersion("1.0")]
[Authorize]
public sealed class ProductsController : ApiControllerBase
{
    [HttpGet]
    [HasPermission(Permissions.Products.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList([FromQuery] GetProductsQuery query, CancellationToken cancellationToken)
        => (await Mediator.Send(query, cancellationToken)).ToActionResult();

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Products.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => (await Mediator.Send(new GetProductByIdQuery(id), cancellationToken)).ToActionResult();

    [HttpGet("summary")]
    [HasPermission(Permissions.Products.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        => (await Mediator.Send(new GetProductSummaryQuery(), cancellationToken)).ToActionResult();

    [HttpGet("low-stock")]
    [HasPermission(Permissions.Products.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10, CancellationToken cancellationToken = default)
        => (await Mediator.Send(new GetLowStockProductsQuery(threshold), cancellationToken)).ToActionResult();

    [HttpPost]
    [HasPermission(Permissions.Products.Create)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
        => (await Mediator.Send(command, cancellationToken)).ToActionResult();

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Products.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Currency, request.CategoryId);
        return (await Mediator.Send(command, cancellationToken)).ToActionResult();
    }

    [HttpPost("{id:int}/stock-adjustments")]
    [HasPermission(Permissions.Products.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] AdjustStockRequest request, CancellationToken cancellationToken)
        => (await Mediator.Send(new AdjustProductStockCommand(id, request.Delta), cancellationToken)).ToActionResult();

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Products.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => (await Mediator.Send(new DeleteProductCommand(id), cancellationToken)).ToActionResult();

    [HttpGet("export/excel")]
    [HasPermission(Permissions.Products.Export)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportExcel([FromQuery] int? categoryId, CancellationToken cancellationToken)
        => (await Mediator.Send(new ExportProductsQuery(categoryId), cancellationToken)).ToActionResult();

    [HttpGet("export/pdf")]
    [HasPermission(Permissions.Products.Export)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPdf(CancellationToken cancellationToken)
        => (await Mediator.Send(new GenerateProductCatalogPdfQuery(), cancellationToken)).ToActionResult();
}

public sealed record UpdateProductRequest(string Name, string? Description, decimal Price, string Currency, int CategoryId);

public sealed record AdjustStockRequest(int Delta);

using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanApi.Api.Controllers;

/// <summary>Base for versioned API controllers. Exposes MediatR and the standard route shape.</summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}

using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CleanApi.Application.Modules.Auth;

public sealed record RegisterCommand(string Email, string Password) : IRequest<Result<string>>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public sealed class RegisterCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, Result<string>>
{
    public Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken) =>
        identityService.RegisterAsync(request.Email, request.Password, cancellationToken);
}

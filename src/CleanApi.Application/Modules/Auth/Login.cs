using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CleanApi.Application.Modules.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthenticationResult>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
{
    public Task<Result<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken) =>
        identityService.LoginAsync(request.Email, request.Password, cancellationToken);
}

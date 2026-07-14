using CleanApi.Application.Common.Interfaces;
using CleanApi.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace CleanApi.Application.Modules.Auth;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthenticationResult>>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public sealed class RefreshTokenCommandHandler(IIdentityService identityService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResult>>
{
    public Task<Result<AuthenticationResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) =>
        identityService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
}

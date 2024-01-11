using Core.Helpers;
using FluentValidation;
using MediatR;

namespace Service.Auth;

public class LoginServiceRequest(string userName, string password) : IRequest<LoginServiceResponse>
{
    public string UserName { get; } = userName;
    public string Password { get; } = password;
}

public class LoginServiceRequestValidator : AbstractValidator<LoginServiceRequest>
{
    public LoginServiceRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage($"{nameof(LoginServiceRequest.UserName)}_should_not_be_empty");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage($"{nameof(LoginServiceRequest.Password)}_should_not_be_empty");
    }
}

public class LoginServiceRequestHandler(ITokenHelper tokenHelper)
    : IRequestHandler<LoginServiceRequest, LoginServiceResponse>
{
    public async Task<LoginServiceResponse> Handle(LoginServiceRequest request, CancellationToken cancellationToken)
    {
        //validate given username and password with your custom logic
        
        var tokenResponse = tokenHelper.GenerateToken(Guid.NewGuid(), request.UserName);

        return new LoginServiceResponse
        {
            Type = tokenResponse.Type,
            Token = tokenResponse.Token,
            ExpireIn = tokenResponse.ExpireIn
        };
    }
}

public class LoginServiceResponse
{
    public string Type { get; set; }
    public string Token { get; set; }
    public int ExpireIn { get; set; }
}
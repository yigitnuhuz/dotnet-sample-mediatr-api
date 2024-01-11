using Api.Controllers.V1.Models;
using Asp.Versioning;
using Core.Context;
using Core.Controllers;
using Core.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Auth;


namespace Api.Controllers.V1;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(IMediator mediator, ICoreAppContext appContext) : LoginBaseController
{
    /// <summary>
    /// Endpoint for logging in a user with the provided credentials.
    /// </summary>
    /// <remarks>
    /// This API endpoint can be used to authenticate a user using their login credentials. The user's credentials 
    /// should be sent in a request body using the `LoginModel` model. If authentication is successful, the endpoint 
    /// will return a `LoginServiceResponse` containing a Bearer token in JWT format that the user can use to 
    /// authenticate subsequent requests.
    /// </remarks>
    /// <param name="model">The model containing the user's login credentials and company ID.</param>
    /// <returns>A response containing the Bearer token in JWT format and additional user information.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginServiceResponse), 200)]
    [ProducesResponseType(typeof(ApiErrorDto), 400)]
    public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
    {
        var response = await mediator.Send(new LoginServiceRequest(model.Username, model.Password), CancellationToken.None);
        return Ok(response);
    }

    /// <summary>
    /// Endpoint for validating the authenticity of a user's token.
    /// </summary>
    /// <remarks>
    ///  This API endpoint can be used to validate the authenticity of a user's token. If the token is valid,
    /// a success response will be returned along with additional user information.
    /// </remarks>
    /// <returns>A response containing username with valid message.</returns>
    [HttpGet("validate")]
    [ProducesResponseType( 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ValidateToken()
    {
        return Ok($"your token is valid {appContext.UserName()}");
    }
}
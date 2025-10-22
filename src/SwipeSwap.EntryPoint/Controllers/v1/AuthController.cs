using EntryPoint.Dtos;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SwipeSwap.Domain.Exceptions;
using RegisterUserRequest = SwipeSwap.Application.Auth.Dtos.RegisterUserRequest;
using LoginUserRequest = SwipeSwap.Application.Auth.Dtos.LoginUserRequest;
using RefreshTokenRequest = SwipeSwap.Application.Auth.Dtos.RefreshTokenRequest;

namespace EntryPoint.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")] 
public class AuthenticateController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var registerRequest = new RegisterUserRequest(request.Email, request.Password, request.DisplayName);
            var token = await mediator.Send(registerRequest, cancellationToken);
            return Ok(new AuthResponse { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
        }
        catch (BusinessException ex)
        {
            return Conflict(new AuthResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new AuthResponse(ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(Dtos.LoginUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var loginRequest = new LoginUserRequest(request.Email, request.Password);
            var token = await mediator.Send(loginRequest, cancellationToken);
            return Ok(new AuthResponse { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new AuthResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new AuthResponse(ex.Message));
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(Dtos.RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var refreshRequest = new RefreshTokenRequest(request.RefreshToken);
            var token = await mediator.Send(refreshRequest, cancellationToken);
            return Ok(new AuthResponse { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new AuthResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new AuthResponse(ex.Message));
        }
    }
}
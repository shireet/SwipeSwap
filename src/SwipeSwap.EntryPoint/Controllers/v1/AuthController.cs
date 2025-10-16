using EntryPoint.Dtos;
using FluentValidation.AspNetCore;
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
    public async Task<IActionResult> Register(Dtos.RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterUserRequest(request.Email, request.Password, request.DisplayName);
        var token = await mediator.Send(registerRequest, cancellationToken);
        return Ok(new AuthResponse { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(Dtos.LoginUserRequest request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginUserRequest(request.Email, request.Password);
        var token = await mediator.Send(loginRequest, cancellationToken);
        return Ok(new AuthResponse { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(Dtos.RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var refreshRequest = new RefreshTokenRequest(request.RefreshToken);
        var token = await mediator.Send(refreshRequest, cancellationToken);
        return Ok(new AuthResponse { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
    }
}
using EntryPoint.Dtos;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SwipeSwap.Domain.Exceptions;

namespace EntryPoint.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var registerRequest = new SwipeSwap.Application.Auth.RegisterUserRequest(request.Email, request.Password, request.DisplayName);
        try
        {
            var token = await mediator.Send(registerRequest, cancellationToken);
            return Ok(new RegisterResponse { Token = token });
        }
        catch (BusinessException ex)
        {
            return Conflict(new RegisterResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new RegisterResponse(ex.Message));
        }
    }
}
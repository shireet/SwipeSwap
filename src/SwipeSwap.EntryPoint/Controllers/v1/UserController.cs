using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwipeSwap.Application.Profile.Dtos;

namespace EntryPoint.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/user")] 
public class UserController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
    {
        var request = new GetUserByIdRequest(id);
        var user = await mediator.Send(request, cancellationToken);
        return Ok(new EntryPoint.Dtos.UserDto(user.Id, user.Email, user.Email));
    }
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await mediator.Send(new GetCurrentUserRequest(userId), cancellationToken);
        return Ok(user);
    }
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(EntryPoint.Dtos.UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}
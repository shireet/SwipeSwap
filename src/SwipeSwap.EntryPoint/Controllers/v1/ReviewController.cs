using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryPoint.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/review")]
public class ReviewController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> LeaveReview(CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        return Ok();
    }
    [HttpGet("user/{id:int}")]
    public async Task<IActionResult> LookReviews(int id, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        return Ok();
    }
    [HttpGet("user/me")]
    public async Task<IActionResult> LookMyReviews(CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        return Ok();
    }
}
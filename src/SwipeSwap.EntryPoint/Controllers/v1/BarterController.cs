using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryPoint.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/barter")]
public class BarterController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> ProposeBarter(CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        return Ok();
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBarterDetails(int id, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        return Ok();
    }
    
    [HttpPut("{id:int}/accept")]
    public async Task<IActionResult> AcceptBarter(int id, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        return Ok();
    }
    
    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> RejectBarter(int id, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        return Ok();
    }
    
    [HttpPut("barters")]
    public async Task<IActionResult> GetBarters(int id, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken);
        return Ok();
    }
}
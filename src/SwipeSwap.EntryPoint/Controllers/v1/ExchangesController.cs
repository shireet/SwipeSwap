using MediatR;
using Microsoft.AspNetCore.Mvc;
using SwipeSwap.Application.Exchanges;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.EntryPoint.Dtos.Exchanges;

namespace EntryPoint.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ExchangesController(IMediator mediator) : ControllerBase
{
    [HttpPost("{id:int}/accept")]
    public async Task<ActionResult<ExchangeDto>> Accept(int id, [FromBody] AcceptExchange body, CancellationToken ct)
    {
        var dto = await mediator.Send(new AcceptExchangeRequest(id, body.ActorUserId), ct);
        return Ok(dto);
    }

    [HttpPost("{id:int}/decline")]
    public async Task<ActionResult<ExchangeDto>> Decline(int id, [FromBody] DeclineExchange body, CancellationToken ct)
    {
        var dto = await mediator.Send(new DeclineExchangeRequest(id, body.ActorUserId, body.Reason), ct);
        return Ok(dto);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<ExchangeDto>> Cancel(int id, [FromBody] CancelExchange body, CancellationToken ct)
    {
        var dto = await mediator.Send(new CancelExchangeRequest(id, body.ActorUserId, body.Reason), ct);
        return Ok(dto);
    }

    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<ExchangeDto>> Complete(int id, [FromBody] CompleteExchange body, CancellationToken ct)
    {
        var dto = await mediator.Send(new CompleteExchangeRequest(id, body.ActorUserId, body.Note), ct);
        return Ok(dto);
    }
    
    [HttpPost]
    public async Task<ActionResult<ExchangeDto>> Create([FromBody] CreateExchangeDto body, CancellationToken ct)
    {
        var dto = await mediator.Send(new CreateExchangeRequest(
            body.InitiatorUserId,
            body.OfferedItemId,
            body.RequestedItemId,
            body.Message
        ), ct);

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExchangeDto>> GetById(int id, CancellationToken ct)
    {
        return Ok(); 
    }
}

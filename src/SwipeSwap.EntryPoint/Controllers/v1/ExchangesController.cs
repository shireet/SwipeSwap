using MediatR;
using Microsoft.AspNetCore.Mvc;
using SwipeSwap.Application.Exchanges;
using SwipeSwap.Application.Exchanges.Dtos;

namespace EntryPoint.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ExchangesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ExchangeDto>> Create([FromBody] CreateExchangeDto body, CancellationToken ct)
    {
        var dto = await mediator.Send(new CreateExchangeCommand(
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

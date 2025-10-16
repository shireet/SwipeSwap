

using System.Security.Claims;
using EntryPoint.Dtos.items;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SwipeSwap.Application.Items;

namespace EntryPoint.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class ItemsController(IMediator mediator) : ControllerBase
{
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub"); 
        if (string.IsNullOrWhiteSpace(idClaim))
            return Unauthorized();

        if (!int.TryParse(idClaim, out var ownerId))
            return Unauthorized("Invalid user id in token.");

        var cmd = new DeleteItemRequest(id, ownerId);
        var result = await mediator.Send(cmd);
        if (!result) return NotFound();
        return NoContent();
    }
    
    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update([FromBody] UpdateItem updateItem, [FromQuery] int id)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub"); 
        if (string.IsNullOrWhiteSpace(idClaim))
            return Unauthorized();

        if (!int.TryParse(idClaim, out var ownerId))
            return Unauthorized("Invalid user id in token.");

        var cmd = new UpdateItemRequest(id, ownerId, updateItem.Title, updateItem.Description, updateItem.IsActive, updateItem.Tags);
        var result = await mediator.Send(cmd);
        if (!result) return NotFound();
        return NoContent();
    }
    
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] CreateItem createItem)
    {

        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub"); 
        if (string.IsNullOrWhiteSpace(idClaim))
            return Unauthorized();

        if (!int.TryParse(idClaim, out var ownerId))
            return Unauthorized("Invalid user id in token.");

        var cmd = new CreateItemRequest(ownerId, createItem.Title, createItem.Description, createItem.Tags ?? new());
        var itemId = await mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { id = itemId }, new { id = itemId });
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await mediator.Send(new GetItemByIdQuery(id));
        if (item is null) return NotFound();
        return Ok(item);
    }       
}
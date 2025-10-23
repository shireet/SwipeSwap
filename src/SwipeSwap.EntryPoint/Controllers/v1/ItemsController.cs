using System.Security.Claims;
using EntryPoint.Dtos.items;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SwipeSwap.Application.Items.Dtos;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using SwipeSwap.Application.Items;

namespace EntryPoint.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]/[action]")]
public class ItemsController(IMediator mediator) : ControllerBase
{
    [HttpGet("recommended")]
    public async Task<IActionResult> Recommended(
        [FromQuery] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!int.TryParse(idClaim, out var userId)) return Unauthorized("Произошла ошибка при авторизации.");

        var mockResult = new List<CatalogItem>
        {
            new(
                Id: 1,
                Title: "Гитара Fender",
                Description: "Электрогитара в отличном состоянии",
                City: "Москва",
                CategoryId: 2,
                Condition: SwipeSwap.Domain.Models.Enums.ItemCondition.Good,
                Tags: new[] { "музыка", "инструменты" }
            ),
            new(
                Id: 2,
                Title: "Велосипед Cube",
                Description: "Почти новый горный велосипед",
                City: "Санкт-Петербург",
                CategoryId: 3,
                Condition: SwipeSwap.Domain.Models.Enums.ItemCondition.New,
                Tags: new[] { "спорт", "транспорт" }
            ),
            new(
                Id: 3,
                Title: "Книга C# для начинающих",
                Description: "Отличное состояние, мягкая обложка",
                City: "Казань",
                CategoryId: 4,
                Condition: SwipeSwap.Domain.Models.Enums.ItemCondition.Good,
                Tags: new[] { "книги", "программирование" }
            )
        };
        return Ok(mockResult);
    }

    
    [HttpGet("catalog")]
    public async Task<IActionResult> Catalog(
        [FromQuery] int page = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        [FromQuery] string? sortBy = null,     
        [FromQuery] string? sortDir = null,   
        [FromQuery] int? categoryId = null,
        [FromQuery] int? condition = null,    
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        [FromQuery] string[]? tags = null,
        [FromQuery] bool onlyActive = true)
    {
        var res = await mediator.Send(new GetCatalogQuery(
            page, pageSize, sortBy, sortDir, categoryId,
            condition is null ? null : (SwipeSwap.Domain.Models.Enums.ItemCondition?)condition,
             city, search, tags, onlyActive));

        return Ok(res);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(idClaim))
            return Unauthorized();

        if (!int.TryParse(idClaim, out var ownerId))
            return Unauthorized("Произошла ошибка при авторизации.");

        var cmd = new DeleteItemRequest(id, ownerId);
        var result = await mediator.Send(cmd);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update([FromBody] UpdateItem updateItem, [FromQuery] int id)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(idClaim)) return Unauthorized();
        if (!int.TryParse(idClaim, out var ownerId)) return Unauthorized("Произошла ошибка при авторизации.");

        var cmd = new UpdateItemRequest(
            id: id,
            OwnerId: ownerId,
            Title: updateItem.Title,
            Description: updateItem.Description,
            IsActive: updateItem.IsActive,
            Tags: updateItem.Tags,
            Condition: updateItem.Condition,
            City: updateItem.City
        );

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
            return Unauthorized("Произошла ошибка при авторизации.");

        var cmd = new CreateItemRequest(ownerId, createItem.Title, createItem.Description, createItem.ImageUrl,  createItem.Tags ?? new(), createItem.Condition, createItem.City);
        var itemId = await mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { id = itemId }, new { id = itemId });
    }

    [HttpGet]
    public async Task<IActionResult> GetByUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(idClaim))
            return Unauthorized();

        if (!int.TryParse(idClaim, out var ownerId))
            return Unauthorized("Произошла ошибка при авторизации.");

        var result = await mediator.Send(new GetItemsByOwnerRequest(ownerId));
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await mediator.Send(new GetItemByIdQuery(id));
        if (item is null) return NotFound();
        return Ok(item);
    }
}
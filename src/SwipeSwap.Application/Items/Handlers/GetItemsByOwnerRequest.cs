using MediatR;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Application.Items.Handlers;

public sealed class GetItemsByOwnerHandler(IItemRepository items)
    : IRequestHandler<GetItemsByOwnerRequest, List<ItemDto>>
{
    public async Task<List<ItemDto>> Handle(GetItemsByOwnerRequest req, CancellationToken ct)
    {
        var list = await items.GetByOwnerAsync(req.OwnerId, ct);

        return [.. list.Select(i => new ItemDto(
            Id: i.Id,
            OwnerId: i.OwnerId,
            Title: i.Title,
            Description: i.Description,
            Tags: i.ItemTags.Select(it => it.Tag.Name).ToList() 
        ))];
    }
}

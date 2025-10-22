using MediatR;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Items.Handlers;

public class GetItemByIdHandler(IItemRepository repo)
    : IRequestHandler<GetItemByIdQuery, ItemDto?>
{
    public async Task<ItemDto?> Handle(GetItemByIdQuery q, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(q.Id);
        if (item is null) return null;

        return new ItemDto(
            item.Id,
            item.OwnerId,
            item.Title,
            item.Description,
            item.ItemTags.Select(it => it.Tag.Name).ToList()
        );
    }
}
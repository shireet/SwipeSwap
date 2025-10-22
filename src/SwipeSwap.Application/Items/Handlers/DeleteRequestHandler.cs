using MediatR;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Application.Items;

public class DeleteRequestHandler(IItemRepository itemRepository) : IRequestHandler<DeleteItemRequest, bool>
{
    public async Task<bool> Handle(DeleteItemRequest request, CancellationToken cancellationToken)
    {
        var item = await itemRepository.GetByIdAsync(request.ItemId);
        if (item == null || item.OwnerId != request.OwnerId)
            return false;

        item.IsActive = false;
        await itemRepository.UpsertAsync(item);
        return true;
    }
}
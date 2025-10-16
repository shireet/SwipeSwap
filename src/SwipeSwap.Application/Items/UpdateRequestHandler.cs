using MediatR;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Application.Items;

public class UpdateRequestHandler(IItemRepository repo)
    : IRequestHandler<UpdateItemRequest, bool>
{
    public async Task<bool> Handle(UpdateItemRequest request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.id);
        if (item is null || item.OwnerId != request.OwnerId) return false;

        item.Title = request.Title;
        item.Description = request.Description;
        if (request.IsActive is bool isActive)
            item.IsActive = isActive;

        var existingTags = item.ItemTags.Select(it => it.Tag.Name).ToList();
        var tagsToAdd = request.Tags.Except(existingTags).ToList();
        var tagsToRemove = existingTags.Except(request.Tags).ToList();

        foreach (var tag in tagsToAdd)
        {
            await repo.AddTagToItemAsync(item.Id, tag);
        }

        foreach (var tag in tagsToRemove)
        {
            await repo.RemoveTagFromItemAsync(item.Id, tag);
        }

        await repo.UpsertAsync(item);
        return true;
    }
}

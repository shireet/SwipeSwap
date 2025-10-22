using MediatR;
using SwipeSwap.Application.Items;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

public class UpdateRequestHandler(IItemRepository repo)
    : IRequestHandler<UpdateItemRequest, bool>
{
    public async Task<bool> Handle(UpdateItemRequest request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.id, ct);
        if (item is null || item.OwnerId != request.OwnerId)
            return false;

        if (request.Title is not null)
            item.Title = request.Title;

        if (request.Description is not null)
            item.Description = request.Description;

        if (request.IsActive is bool isActive)
            item.IsActive = isActive;

        if (!string.IsNullOrWhiteSpace(request.City))
            item.City = request.City;

        if (request.Condition is not null)
            item.Condition = request.Condition.Value;

        if (request.Tags is not null)
        {
            var reqTags = request.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingTags = item.ItemTags.Select(it => it.Tag.Name).ToList();

            var tagsToAdd = reqTags.Except(existingTags, StringComparer.OrdinalIgnoreCase);
            var tagsToRemove = existingTags.Except(reqTags, StringComparer.OrdinalIgnoreCase);

            foreach (var tag in tagsToAdd)
                await repo.AddTagToItemAsync(item.Id, tag);

            foreach (var tag in tagsToRemove)
                await repo.RemoveTagFromItemAsync(item.Id, tag);
        }

        await repo.UpsertAsync(item, ct);
        return true;
    }
}
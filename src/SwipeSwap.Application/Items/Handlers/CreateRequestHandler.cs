using MediatR;
using SwipeSwap.Application.Items.Dtos;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Postgres.Repositories.Interfaces;

namespace SwipeSwap.Application.Items.Handlers
{
    public class CreateRequestHandler : IRequestHandler<CreateItemRequest, int>
    {
        private readonly IItemRepository _items;
        private readonly IUserRepository _users;

        public CreateRequestHandler(IItemRepository items, IUserRepository users)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public async Task<int> Handle(CreateItemRequest req, CancellationToken cancellationToken)
        {
            var ownerExists = await _users.GetUserAsync(req.OwnerId, cancellationToken);
            if (ownerExists == null)
                throw new InvalidOperationException("Пользователь не существует.");

            var normalizedTags = (req.Tags ?? new List<string>())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var entity = new Item
            {
                OwnerId = req.OwnerId,
                Title = req.Title,
                Description = req.Description,
                ImageUrl = req.ImageUrl,
                Condition = req.Condition,                                   
                City = string.IsNullOrWhiteSpace(req.City) ? null : req.City.Trim(), 
                ItemTags = normalizedTags
                    .Select(name => new ItemTag { Tag = new Tag { Name = name } })
                    .ToList()
            };

            var newId = await _items.UpsertAsync(entity);
            return newId;
        }
    }
}
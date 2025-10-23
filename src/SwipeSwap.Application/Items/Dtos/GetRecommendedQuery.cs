using MediatR;
using SwipeSwap.Application.Items.Dtos;

namespace SwipeSwap.Application.Items;

public sealed record GetRecommendedQuery(
    int UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<IReadOnlyList<CatalogItem>>;
using MediatR;
using SwipeSwap.Application.Common.Exceptions;
using SwipeSwap.Application.Exchanges.Dtos;
using SwipeSwap.Domain.Models;
using SwipeSwap.Infrastructure.Repositories.Interfaces;

namespace SwipeSwap.Application.Exchanges;

public class CreateExchangeHandler(
    IItemRepository items,
    IExchangeRepository exchanges
) : IRequestHandler<CreateExchangeCommand, ExchangeDto>
{
    public async Task<ExchangeDto> Handle(CreateExchangeCommand req, CancellationToken ct)
    {
        var me = req.InitiatorUserId;

        var offered = await items.GetByIdAsync(req.OfferedItemId, ct)
            ?? throw new NotFoundException($"Предмет для предложения с ID {req.OfferedItemId} не найден.");
        var requested = await items.GetByIdAsync(req.RequestedItemId, ct)
            ?? throw new NotFoundException($"Запрашиваемый предмет с ID {req.RequestedItemId} не найден.");

        if (offered.OwnerId != me)
            throw new ForbiddenException("Вы можете предлагать только те предметы, которыми владеете.");
        if (requested.OwnerId == me)
            throw new ValidationException("Нельзя отправлять обмен самому себе.");
        if (offered.Id == requested.Id)
            throw new ValidationException("Предлагаемый и запрашиваемый предметы должны быть разными.");
        if (!offered.IsActive)
            throw new ValidationException("Ваш предмет недоступен для обмена.");
        if (!requested.IsActive)
            throw new ValidationException("Запрашиваемый предмет недоступен для обмена.");

        if (await exchanges.ExistsOpenForPairAsync(me, offered.Id, requested.Id, ct))
            throw new ValidationException("Вы уже отправили активный оффер для этой пары предметов.");

        var entity = Exchange.Create(me, requested.OwnerId, offered.Id, requested.Id, req.Message);

        await exchanges.AddAsync(entity, ct);
        await exchanges.SaveChangesAsync(ct); 

        return new ExchangeDto(
            entity.Id,
            entity.InitiatorId,
            entity.ReceiverId,
            entity.OfferedItemId,
            entity.RequestedItemId,
            entity.Status.ToString(), 
            entity.CreatedAt
        );
    }
}
namespace SwipeSwap.Application.Items.Dtos;

public record ItemDto(
    int Id,
    int OwnerId,
    string Title,
    string? Description,
    List<string> Tags
);
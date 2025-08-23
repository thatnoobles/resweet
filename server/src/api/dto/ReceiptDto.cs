using System;

namespace Resweet.Api.DataTransferObjects;

public record ReceiptDto : Dto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public DateTime DateCreated { get; init; }
    public UserDto UserPaid { get; init; }
    public UserDto UserCreated { get; init; }
    public ReceiptItemDto[] Items { get; init; }
}

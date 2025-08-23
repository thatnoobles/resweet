using System;

namespace Resweet.Api.DataTransferObjects;

public record ReceiptItemDto : Dto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public float Price { get; init; }
    public UserDto[] UsersSharing { get; init; }
}

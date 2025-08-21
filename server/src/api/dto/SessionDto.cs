using System;

namespace Resweet.Api.DataTransferObjects;

public record SessionDto : Dto
{
    public string SessionToken { get; init; }
    public DateTime ExpirationDate { get; init; }
}

namespace Resweet.Api.DataTransferObjects;

public record UserDto : Dto
{
    public string DisplayName { get; init; }
    public string Handle { get; init; }
    public string VenmoLogin { get; init; }
}

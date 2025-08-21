namespace Resweet.Api.DataTransferObjects;

public record GroupDto : Dto
{
    public string Name { get; init; }
    public UserDto[] Users { get; init; }
    public string InviteCode { get; init; }
}

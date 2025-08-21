using Newtonsoft.Json;
using Resweet.Database.Entities;

namespace Resweet.Api.DataTransferObjects;

public abstract record Dto
{
    public string ToJson() => JsonConvert.SerializeObject(this);
}

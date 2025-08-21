using Npgsql;
using Resweet.Api.DataTransferObjects;

namespace Resweet.Database.Entities;

public interface Entity<T>
    where T : Dto
{
    public T ToDto();

    public void PopulateFromReader(NpgsqlDataReader reader);
}

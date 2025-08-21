using Npgsql;
using Resweet.Api.DataTransferObjects;

namespace Resweet.Database.Entities;

public interface Entity
{
    public void PopulateFromReader(NpgsqlDataReader reader);
}

public interface Entity<T> : Entity
    where T : Dto
{
    public T ToDto();
}

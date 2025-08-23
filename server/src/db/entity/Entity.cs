using System;
using Npgsql;
using Resweet.Api.DataTransferObjects;

namespace Resweet.Database.Entities;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    public abstract void PopulateFromReader(NpgsqlDataReader reader);

    public override bool Equals(object obj)
    {
        if (obj.GetType() != GetType())
            return false;

        Entity other = (Entity)obj;
        return other.Id == Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}

public abstract class Entity<T> : Entity
    where T : Dto
{
    public abstract T ToDto();
}

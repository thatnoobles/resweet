using System;
using System.Collections.Generic;
using Npgsql;
using Resweet.Database.Entities;

namespace Resweet.Database.Utils;

public static class DatabaseUtils
{
    private static NpgsqlDataSource dataSource;

    public static void Connect()
    {
        string connectionString = Configuration.ConnectionString;
        dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public static T SelectOne<T>(string query, params object[] args)
        where T : Entity, new()
    {
        if (dataSource == null)
            throw new NullReferenceException(
                "PGSQL data source is null (has it  been initialized?)"
            );

        NpgsqlCommand cmd = BuildCommand(query, args);
        using (NpgsqlDataReader reader = cmd.ExecuteReader())
        {
            if (!reader.Read())
                return default;

            T obj = new();
            obj.PopulateFromReader(reader);
            return obj;
        }
    }

    public static T[] SelectMany<T>(string query, params object[] args)
        where T : Entity, new()
    {
        if (dataSource == null)
            throw new NullReferenceException(
                "PGSQL data source is null (has it  been initialized?)"
            );

        List<T> result = new List<T>();

        NpgsqlCommand cmd = BuildCommand(query, args);
        using (NpgsqlDataReader reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                T obj = new();
                obj.PopulateFromReader(reader);
                result.Add(obj);
            }
        }

        return result.ToArray();
    }

    public static void Execute(string query, params object[] args)
    {
        if (dataSource == null)
            throw new NullReferenceException(
                "PGSQL data source is null (has it  been initialized?)"
            );

        NpgsqlCommand cmd = BuildCommand(query, args);
        cmd.ExecuteNonQuery();
    }

    public static T ExecuteReturning<T>(string query, params object[] args)
    {
        if (dataSource == null)
            throw new NullReferenceException(
                "PGSQL data source is null (has it  been initialized?)"
            );

        NpgsqlCommand cmd = BuildCommand(query, args);
        using (NpgsqlDataReader reader = cmd.ExecuteReader())
        {
            if (!reader.Read())
                return default;

            return reader.GetFieldValue<T>(0);
        }
    }

    private static NpgsqlCommand BuildCommand(string query, params object[] args)
    {
        NpgsqlCommand cmd = dataSource.CreateCommand(query);

        foreach (object arg in args)
            cmd.Parameters.AddWithValue(arg);

        return cmd;
    }
}

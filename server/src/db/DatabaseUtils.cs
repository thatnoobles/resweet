using System;
using System.Collections.Generic;
using Npgsql;

namespace Resweet.Database;

public static class DatabaseUtils
{
    private static NpgsqlConnection connection;

    public static void Initialize(string hostname, string username, string password, string database)
    {
        string connectionString =
            $"Host={hostname};Username={username};Password={password};Database={database}";

        connection = NpgsqlDataSource.Create(connectionString).OpenConnection();
    }

    public static T SelectOne<T>(string query, NpgsqlParameter[] parameters) where T : Entity, new()
    {
        NpgsqlCommand command = new NpgsqlCommand(query) { Connection = connection };
        command.Parameters.AddRange(parameters);
        object[] fields;

        using (NpgsqlDataReader reader = command.ExecuteReader())
        {
            if (!reader.HasRows)
                return default;

            fields = new object[reader.FieldCount];
            string[] columnNames = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
                columnNames[i] = reader.GetName(i);

            reader.Read();

            for (int i = 0; i < reader.FieldCount; i++)
                fields[i] = reader[columnNames[i]];

        }

        T obj = new T();
        obj.BuildFromFields(fields);
        return obj;
    }

    public static T[] SelectMany<T>(string query, NpgsqlParameter[] parameters) where T : Entity, new()
    {
        NpgsqlCommand command = new NpgsqlCommand(query) { Connection = connection };
        command.Parameters.AddRange(parameters);

        List<T> objs = new List<T>();

        using (NpgsqlDataReader reader = command.ExecuteReader())
        {
            if (!reader.HasRows)
                return [];

            string[] columnNames = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
                columnNames[i] = reader.GetName(i);

            while (reader.Read())
            {
                object[] fields = new object[reader.FieldCount];

                for (int i = 0; i < reader.FieldCount; i++)
                    fields[i] = reader[columnNames[i]];

                T obj = new T();
                obj.BuildFromFields(fields);
                objs.Add(obj);
            }
        }

        return objs.ToArray();
    }

    public static void Execute(string query, NpgsqlParameter[] parameters)
    {
        NpgsqlCommand command = new NpgsqlCommand(query) { Connection = connection };
        command.Parameters.AddRange(parameters);
        command.ExecuteNonQuery();
    }
}
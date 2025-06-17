using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Npgsql;
using Resweet.Database;
using Resweet.Entities;

namespace Resweet.Api;

public static class ReceiptItemApi
{
    public static IResult CreateReceiptItem(string name, double price)
    {
        string query = @"
        INSERT INTO receipt_items (name, price)
        VALUES ((@name), (@price))
        ";

        NpgsqlParameter[] parameters = [new("name", name), new("price", price)];

        DatabaseUtils.Execute(query, parameters);
        return Results.Ok();
    }

    public static IResult GetReceiptItems(string id)
    {
        string query = @"
        SELECT id, name, price
        FROM receipt_items
        ";

        if (id != "")
            query += "WHERE id = (@id)";

        ReceiptItem[] items = DatabaseUtils.SelectMany<ReceiptItem>(query, id != "" ?
            [new("id", id)] : []
        );

        return Results.Ok(items);
    }

    public static IResult UpdateReceiptItem(string id, string name, double price)
    {
        if (name == "" && price == 0)
            return Results.Ok();

        string query = @"
        UPDATE receipt_items
        SET name = (@name), price = (@price)
        WHERE id = (@id)";

        List<NpgsqlParameter> parameters = [
            new("name", name),
            new("price", price),
            new("id", new Guid(id)
        )];

        DatabaseUtils.Execute(query, parameters.ToArray());
        return Results.Ok();
    }
}
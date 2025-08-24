using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Resweet.Api.Utils;
using Resweet.Database.Entities;

namespace Resweet.Api.Controllers;

public static class ReceiptController
{
    public static IResult Create(JsonObject payload, string sessionToken)
    {
        if (!payload.ContainsKeys("name", "items", "userPaid"))
            return Results.BadRequest();

        string name = (string)payload["name"];
        string userPaidHandle = (string)payload["userPaid"];
        ReceiptItemPayload[] receiptItems = payload["items"]
            .AsArray()
            .Select(node => node.Deserialize<ReceiptItemPayload>())
            .ToArray();

        User user = Session.Authenticate(sessionToken);

        if (user == null)
            return Results.Unauthorized();

        User userPaid = User.GetByHandle(userPaidHandle);

        Receipt receipt = Receipt.Create(name, userPaid, user);

        Console.WriteLine("ITEMS:");
        Console.WriteLine(string.Join("\n", receiptItems));
        receipt.AddItems(
            receiptItems
                .Select(item =>
                    ReceiptItem.Create(
                        item.Name,
                        item.Price,
                        item.UsersSharing.Select(User.GetByHandle).ToArray()
                    )
                )
                .ToArray()
        );

        return Results.Ok(receipt.ToDto());
    }

    public static IResult GetById(Guid id, string sessionToken)
    {
        User user = Session.Authenticate(sessionToken);

        if (user == null)
            return Results.Unauthorized();

        Receipt receipt = Receipt.GetById(id);

        if (receipt == null)
            return Results.NotFound();

        Group[] userGroups = Group.GetByUser(user);

        // Only allow user to view receipts from their own group
        if (!userGroups.Contains(receipt.GetGroup()))
            return Results.NotFound();

        return Results.Ok(receipt.ToDto());
    }

    public static IResult EditReceipt(Guid id, JsonObject payload, string sessionToken)
    {
        if (!payload.ContainsKeys("name", "userPaid"))
            return Results.BadRequest();

        User user = Session.Authenticate(sessionToken);

        if (user == null)
            return Results.Unauthorized();

        Receipt receipt = Receipt.GetById(id);

        if (receipt == null)
            return Results.NotFound();

        Group[] userGroups = Group.GetByUser(user);

        // Only allow user to edit receipts from their own group
        if (!userGroups.Contains(receipt.GetGroup()))
            return Results.NotFound();

        string name = (string)payload["name"];
        User userPaid = User.GetByHandle((string)payload["userPaid"]);

        receipt.Edit(name, userPaid);
        return Results.Ok(receipt.ToDto());
    }

    public static IResult EditReceiptItem(Guid id, JsonObject payload, string sessionToken)
    {
        if (!payload.ContainsKeys("name", "price", "usersSharing"))
            return Results.BadRequest();

        User user = Session.Authenticate(sessionToken);

        if (user == null)
            return Results.Unauthorized();

        ReceiptItem item = ReceiptItem.GetById(id);

        if (item == null)
            return Results.NotFound();

        Group[] userGroups = Group.GetByUser(user);

        // Only allow user to edit receipt items from their own group
        if (!userGroups.Contains(item.GetReceipt().GetGroup()))
            return Results.NotFound();

        ReceiptItemPayload itemPayload = payload.Deserialize<ReceiptItemPayload>();

        item.Edit(
            itemPayload.Name,
            itemPayload.Price,
            itemPayload.UsersSharing.Select(User.GetByHandle).ToArray()
        );
        return Results.Ok(item.ToDto());
    }

    public static IResult DeleteReceipt(Guid id, string sessionToken)
    {
        User user = Session.Authenticate(sessionToken);

        if (user == null)
            return Results.Unauthorized();

        Receipt receipt = Receipt.GetById(id);

        if (receipt == null)
            return Results.NotFound();

        Group[] userGroups = Group.GetByUser(user);

        // Only allow user to delete receipts from their own group
        if (!userGroups.Contains(receipt.GetGroup()))
            return Results.NotFound();

        Receipt.Delete(id);
        return Results.Ok();
    }
}

public struct ReceiptItemPayload
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("price")]
    public float Price { get; set; }

    [JsonPropertyName("usersSharing")]
    public string[] UsersSharing { get; set; } // (handles)

    public override string ToString() => $"{Name} (${Price}) [{string.Join(", ", UsersSharing)}]";
}

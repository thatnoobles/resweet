using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Resweet.Api.DataTransferObjects;
using Resweet.Database.Utils;

namespace Resweet.Database.Entities;

public class ReceiptItem : Entity<ReceiptItemDto>
{
    private string name;
    private float price;

    public override ReceiptItemDto ToDto() =>
        new ReceiptItemDto
        {
            Id = Id,
            Name = name,
            Price = price,
            UsersSharing = GetUsersSharing().Select(user => user.ToDto()).ToArray(),
        };

    public override void PopulateFromReader(NpgsqlDataReader reader)
    {
        Id = reader.GetFieldValue<Guid>(0);
        name = reader.GetFieldValue<string>(1);
        price = reader.GetFieldValue<float>(2);
    }

    public User[] GetUsersSharing() =>
        DatabaseUtils.SelectMany<User>(
            """
            SELECT u.*
            FROM users_receipt_items uri
            INNER JOIN users u ON u.id = uri.user_id
            WHERE uri.receipt_item_id = ($1) 
            """,
            Id
        );

    public Receipt GetReceipt() =>
        DatabaseUtils.SelectOne<Receipt>(
            """
            SELECT r.*
            FROM receipts_receipt_items rri
            INNER JOIN receipts r ON r.id = rri.receipt_id
            WHERE rri.receipt_item_id = ($1)
            """,
            Id
        );

    public void AddUser(User user) =>
        DatabaseUtils.Execute(
            """
            INSERT INTO users_receipt_items (user_id, receipt_item_id)
            VALUES (($1), ($2))
            """,
            user.Id,
            Id
        );

    public void AddUsers(User[] users)
    {
        if (users.Length == 0)
            return;

        string query = "INSERT INTO users_receipt_items (user_id, receipt_item_id) VALUES ";
        List<Guid> args = new List<Guid>();

        for (int i = 1; i <= users.Length * 2; i += 2)
        {
            query += $"((${i}), (${i + 1})){(i == users.Length * 2 - 1 ? "" : ",")} ";
            args.AddRange([users[(i - 1) / 2].Id, Id]);
        }

        DatabaseUtils.Execute(query, args.Cast<object>().ToArray());
    }

    public void RemoveUser(User user) =>
        DatabaseUtils.Execute(
            """
            DELETE FROM users_receipt_items
            WHERE user_id = ($1) AND receipt_item_id = ($2)
            """,
            user.Id,
            Id
        );

    public void RemoveUsers(User[] users)
    {
        if (users.Length == 0)
            return;

        Guid[] userIds = users.Select(user => user.Id).ToArray();

        DatabaseUtils.Execute(
            """
            DELETE FROM users_receipt_items
            WHERE
                receipt_item_id = ($1)
                AND user_id = ANY(($2))
            """,
            Id,
            userIds
        );
    }

    public void Edit(
        string name = UNSET_STRING,
        float price = UNSET_FLOAT,
        User[] sharingUsers = null
    )
    {
        if (name != UNSET_STRING)
            this.name = name;

        if (price != UNSET_FLOAT)
            this.price = price;

        DatabaseUtils.Execute(
            """
            UPDATE receipt_items
            SET name = ($1), price = ($2)
            WHERE id = ($3)
            """,
            this.name,
            this.price,
            Id
        );

        if (sharingUsers == null)
            return;

        User[] existing = GetUsersSharing();
        RemoveUsers(existing.Except(sharingUsers).ToArray());
        AddUsers(sharingUsers.Except(existing).ToArray());
    }

    public static ReceiptItem Create(string name, float price, User[] sharingUsers)
    {
        // Create receipt item
        Guid newItemId = DatabaseUtils.ExecuteReturning<Guid>(
            """
            INSERT INTO receipt_items (name, price)
            VALUES (($1), ($2))
            RETURNING id
            """,
            name,
            price
        );

        // Assign users to share it
        ReceiptItem item = GetById(newItemId);
        item.AddUsers(sharingUsers);
        return item;
    }

    public static ReceiptItem GetById(Guid id) =>
        DatabaseUtils.SelectOne<ReceiptItem>("SELECT * FROM receipt_items WHERE id = ($1)", id);
}

using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Resweet.Api.DataTransferObjects;
using Resweet.Database.Utils;

namespace Resweet.Database.Entities;

public class Receipt : Entity<ReceiptDto>
{
    private string name;
    private DateTime dateCreated;
    private Guid userPaidId;
    private Guid userCreatedId;

    public override ReceiptDto ToDto() =>
        new ReceiptDto
        {
            Id = Id,
            Name = name,
            DateCreated = dateCreated,
            UserPaid = User.GetById(userPaidId).ToDto(),
            UserCreated = User.GetById(userCreatedId).ToDto(),
            Items = GetItems().Select(item => item.ToDto()).ToArray(),
        };

    public override void PopulateFromReader(NpgsqlDataReader reader)
    {
        Id = reader.GetFieldValue<Guid>(0);
        name = reader.GetFieldValue<string>(1);
        dateCreated = reader.GetFieldValue<DateTime>(2);
        userPaidId = reader.GetFieldValue<Guid>(3);
        userCreatedId = reader.GetFieldValue<Guid>(4);
    }

    public ReceiptItem[] GetItems() =>
        DatabaseUtils.SelectMany<ReceiptItem>(
            """
            SELECT ri.*
            FROM receipts_receipt_items rri
            INNER JOIN receipt_items ri ON rri.receipt_item_id = ri.id
            WHERE rri.receipt_id = ($1)
            """,
            Id
        );

    public void AddItem(ReceiptItem item) =>
        DatabaseUtils.Execute(
            """
            INSERT INTO receipts_receipt_items (receipt_id, receipt_item_id)
            VALUES (($1), ($2))
            """,
            Id,
            item.Id
        );

    public void AddItems(ReceiptItem[] items)
    {
        if (items.Length == 0)
            return;

        string query = "INSERT INTO receipts_receipt_items (receipt_id, receipt_item_id) VALUES ";
        List<Guid> args = new List<Guid>();

        for (int i = 1; i <= items.Length * 2; i += 2)
        {
            query += $"((${i}), (${i + 1})){(i == items.Length * 2 - 1 ? "" : ",")} ";
            args.AddRange([Id, items[(i - 1) / 2].Id]);
        }

        DatabaseUtils.Execute(query, args.Cast<object>().ToArray());
    }

    public void RemoveItem(ReceiptItem item) =>
        DatabaseUtils.Execute(
            """
            DELETE FROM receipts_receipt_items
            WHERE receipt_id = ($1) AND receipt_item_id = ($2)
            """,
            Id,
            item.Id
        );

    public void Edit(string name = UNSET_STRING, User userPaid = null)
    {
        if (name != UNSET_STRING)
            this.name = name;

        if (userPaid != null)
            userPaidId = userPaid.Id;

        DatabaseUtils.Execute(
            """
            UPDATE receipts
            SET name = ($1), user_paid_id = ($2)
            WHERE id = ($3)
            """,
            this.name,
            userPaid.Id,
            Id
        );
    }

    public Group GetGroup() =>
        DatabaseUtils.SelectOne<Group>(
            """
            SELECT g.* FROM receipts r
            INNER JOIN groups_users gu ON r.user_created_id = gu.user_id
            INNER JOIN groups g ON g.id = gu.group_id
            WHERE r.id = ($1)
            """,
            Id
        );

    public static Receipt Create(string name, User userPaid, User userCreated)
    {
        Guid newReceiptId = DatabaseUtils.ExecuteReturning<Guid>(
            """
            INSERT INTO receipts (name, user_paid_id, user_created_id)
            VALUES (($1), ($2), ($3))
            RETURNING id
            """,
            name,
            userPaid.Id,
            userCreated.Id
        );

        return GetById(newReceiptId);
    }

    public static void Delete(Guid id)
    {
        DatabaseUtils.Execute("DELETE FROM receipts WHERE id = ($1)", id);

        // Also delete any receipt items
        DatabaseUtils.Execute(
            """
            DELETE
            FROM receipts_items ri
            USING receipts_receipt_items rri
            WHERE rri.receipt_item_id = ri.id AND rri.receipt_id = ($1)
            """,
            id
        );
        DatabaseUtils.Execute("DELETE FROM receipts_receipt_items WHERE receipt_id = ($1)", id);
    }

    public static Receipt GetById(Guid id) =>
        DatabaseUtils.SelectOne<Receipt>("SELECT * FROM receipts WHERE id = ($1)", id);
}

using System;
using System.Linq;
using System.Security.Cryptography;
using Npgsql;
using Resweet.Api.DataTransferObjects;
using Resweet.Database.Utils;

namespace Resweet.Database.Entities;

public class Group : Entity<GroupDto>
{
    private const int INVITE_CODE_LENGTH = 32;

    public Guid Id { get; private set; }

    private string name;
    private string inviteCode;

    public GroupDto ToDto() =>
        new GroupDto
        {
            Name = name,
            InviteCode = inviteCode,
            Users = GetUsers().Select(user => user.ToDto()).ToArray(),
        };

    public void PopulateFromReader(NpgsqlDataReader reader)
    {
        Id = reader.GetFieldValue<Guid>(0);
        name = reader.GetFieldValue<string>(1);
        inviteCode = reader.GetFieldValue<string>(2);
    }

    public void AddUser(User user)
    {
        if (ContainsUser(user))
            return;

        DatabaseUtils.Execute(
            """
            INSERT INTO groups_users (group_id, user_id)
            VALUES (($1), ($2))
            """,
            Id,
            user.Id
        );
    }

    public void RemoveUser(User user)
    {
        DatabaseUtils.Execute(
            """
            DELETE FROM groups_users
            WHERE group_id = ($1) AND user_id = ($2)
            """,
            Id,
            user.Id
        );
    }

    public User[] GetUsers() =>
        DatabaseUtils.SelectMany<User>(
            """
            SELECT u.*
            FROM groups_users gu
            INNER JOIN users u ON gu.user_id = u.id
            WHERE gu.group_id = ($1)
            """,
            Id
        );

    public bool ContainsUser(User user) =>
        DatabaseUtils.SelectOne<User>(
            """
            SELECT u.*
            FROM groups_users gu
            INNER JOIN users u ON gu.user_id = u.id
            WHERE gu.group_id = ($1) AND u.id = ($2)
            """,
            Id,
            user.Id
        ) != null;

    public static Group Create(string name)
    {
        string inviteCode = "";
        for (int i = 0; i < INVITE_CODE_LENGTH; i++)
            inviteCode += (char)RandomNumberGenerator.GetInt32('a', 'z');

        DatabaseUtils.Execute(
            """
            INSERT INTO groups (name, invite_code)
            VALUES (($1), ($2))
            """,
            name,
            inviteCode
        );

        return GetByInviteCode(inviteCode);
    }

    public static Group GetByInviteCode(string inviteCode) =>
        DatabaseUtils.SelectOne<Group>("SELECT * FROM groups WHERE invite_code = ($1)", inviteCode);

    public static Group[] GetByUser(User user) =>
        DatabaseUtils.SelectMany<Group>(
            """
            SELECT g.*
            FROM groups_users gu
            INNER JOIN groups g ON gu.group_id = g.id
            WHERE gu.user_id = ($1)
            """,
            user.Id
        );

    public static bool AnyGroupContainsUser(User user) =>
        DatabaseUtils.SelectOne<User>(
            """
            SELECT u.*
            FROM groups_users gu
            INNER JOIN users u ON gu.user_id = u.id
            WHERE u.id = ($1)
            """,
            user.Id
        ) != null;
}

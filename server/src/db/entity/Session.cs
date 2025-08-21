using System;
using System.Security.Cryptography;
using Npgsql;
using Resweet.Api.DataTransferObjects;
using Resweet.Database.Utils;

namespace Resweet.Database.Entities;

public class Session : Entity<SessionDto>
{
    private const int SESSION_TOKEN_LENGTH = 64;

    private Guid userId;
    private string sessionToken;
    private DateTime expirationDate;

    public SessionDto ToDto() =>
        new SessionDto { SessionToken = sessionToken, ExpirationDate = expirationDate };

    public void PopulateFromReader(NpgsqlDataReader reader)
    {
        userId = reader.GetFieldValue<Guid>(0);
        sessionToken = reader.GetFieldValue<string>(1);
        expirationDate = reader.GetFieldValue<DateTime>(2);
    }

    public static Session Login(string handle, string password)
    {
        User user = User.GetByHandle(handle);

        if (user == null || !user.ValidatePassword(password))
            return null;

        string sessionToken = "";
        for (int i = 0; i < SESSION_TOKEN_LENGTH; i++)
            sessionToken += (char)RandomNumberGenerator.GetInt32('0', 'Z');

        DatabaseUtils.Execute(
            """
            INSERT INTO sessions (user_id, session_token)
            VALUES (($1), ($2))
            """,
            user.Id,
            sessionToken
        );

        return DatabaseUtils.SelectOne<Session>(
            "SELECT * FROM sessions WHERE session_token = ($1)",
            sessionToken
        );
    }

    public static void Logout(string sessionToken)
    {
        DatabaseUtils.Execute(
            """
            DELETE FROM sessions
            WHERE session_token = ($1)
            """,
            sessionToken
        );
    }

    public static User Authenticate(string sessionToken)
    {
        if (sessionToken == null)
            return null;

        User authUser = DatabaseUtils.SelectOne<User>(
            """
            SELECT users.* FROM sessions
            INNER JOIN users ON users.id = sessions.user_id
            WHERE sessions.session_token = ($1)
            """,
            sessionToken
        );

        // Auth failed
        if (authUser == null)
            return null;

        // Refresh session
        DatabaseUtils.Execute(
            """
            UPDATE sessions
            SET expiration_date = NOW() + INTERVAL '7 days'
            WHERE session_token = ($1)
            """,
            sessionToken
        );

        return authUser;
    }
}

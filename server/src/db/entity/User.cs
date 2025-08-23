using System;
using System.Security.Cryptography;
using Npgsql;
using Resweet.Api.DataTransferObjects;
using Resweet.Database.Utils;

namespace Resweet.Database.Entities;

public class User : Entity<UserDto>
{
    private string displayName;
    private string handle;
    private string passwordHash;
    private byte[] salt;

    public override UserDto ToDto() => new UserDto { DisplayName = displayName, Handle = handle };

    public override void PopulateFromReader(NpgsqlDataReader reader)
    {
        Id = reader.GetFieldValue<Guid>(0);
        displayName = reader.GetFieldValue<string>(1);
        handle = reader.GetFieldValue<string>(2);
        passwordHash = reader.GetFieldValue<string>(3);
        salt = reader.GetFieldValue<byte[]>(4);
    }

    public bool ValidatePassword(string password)
    {
        string passwordHash = Convert.ToBase64String(
            Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 64)
        );

        return this.passwordHash == passwordHash;
    }

    public static User Create(string displayName, string handle, string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        string passwordHash = Convert.ToBase64String(
            Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 64)
        );

        DatabaseUtils.Execute(
            """
            INSERT INTO users (display_name, handle, password_hash, salt)
            VALUES (($1), ($2), ($3), ($4))
            """,
            displayName,
            handle,
            passwordHash,
            salt
        );

        return GetByHandle(handle);
    }

    public static User GetById(Guid id) =>
        DatabaseUtils.SelectOne<User>("SELECT * FROM users WHERE id = ($1)", id);

    public static User GetByHandle(string handle) =>
        DatabaseUtils.SelectOne<User>("SELECT * FROM users WHERE handle = ($1)", handle);
}

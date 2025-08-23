using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Resweet.Api.Utils;
using Resweet.Database.Entities;

namespace Resweet.Api.Controllers;

public static class UserController
{
    public static IResult Create(JsonObject payload)
    {
        if (!payload.ContainsKeys("displayName", "handle", "password"))
            return Results.BadRequest();

        string displayName = (string)payload["displayName"];
        string handle = (string)payload["handle"];
        string password = (string)payload["password"];

        User user = User.GetByHandle(handle);

        if (user != null)
            return Results.Conflict();

        return Results.Ok(User.Create(displayName, handle, password));
    }

    public static IResult GetByHandle(string handle)
    {
        User user = User.GetByHandle(handle);

        if (user == null)
            return Results.NotFound();

        return Results.Ok(user.ToDto());
    }

    public static IResult GetCurrent(string sessionToken)
    {
        User user = Session.Authenticate(sessionToken);

        if (user == null)
            return Results.Unauthorized();

        return Results.Ok(user.ToDto());
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Resweet.Api.Utils;
using Resweet.Database.Entities;

namespace Resweet.Api.Controllers;

public static class UserController
{
    public static IResult Create(JsonElement payload)
    {
        if (!ApiUtils.IsPayloadValid(payload, "displayName", "handle", "password"))
            return Results.BadRequest();

        string displayName = payload.GetProperty("displayName").GetString();
        string handle = payload.GetProperty("handle").GetString();
        string password = payload.GetProperty("password").GetString();

        User user = User.GetByHandle(handle);

        if (user != null)
            return Results.Conflict();

        User.Create(displayName, handle, password);
        return Results.Created();
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

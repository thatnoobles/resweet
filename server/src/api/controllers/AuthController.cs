using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Resweet.Api.Utils;
using Resweet.Database.Entities;

namespace Resweet.Api.Controllers;

public static class AuthController
{
    public static IResult Login(JsonElement payload)
    {
        if (!ApiUtils.IsPayloadValid(payload, "handle", "password"))
            return Results.BadRequest();

        Session session = Session.Login(
            payload.GetProperty("handle").GetString(),
            payload.GetProperty("password").GetString()
        );

        if (session == null)
            return Results.Unauthorized();

        return Results.Ok(session.ToDto());
    }

    public static IResult Logout(string sessionToken)
    {
        if (Session.Authenticate(sessionToken) == null)
            return Results.Unauthorized();

        Session.Logout(sessionToken);
        return Results.Ok();
    }
}

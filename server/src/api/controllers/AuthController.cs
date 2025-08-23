using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Resweet.Api.Utils;
using Resweet.Database.Entities;

namespace Resweet.Api.Controllers;

public static class AuthController
{
    public static IResult Login(JsonObject payload)
    {
        if (!payload.ContainsKeys("handle", "password"))
            return Results.BadRequest();

        Session session = Session.Login((string)payload["handle"], (string)payload["password"]);

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

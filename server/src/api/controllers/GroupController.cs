using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Resweet.Api.Utils;
using Resweet.Database.Entities;

namespace Resweet.Api.Controllers;

public static class GroupController
{
    public static IResult Create(JsonObject payload, string sessionToken)
    {
        User user = Session.Authenticate(sessionToken);

        if (user == null)
            return Results.Unauthorized();

        if (payload.ContainsKeys("name"))
            return Results.BadRequest();

        Group group = Group.Create((string)payload["name"]);
        group.AddUser(user);
        return Results.Ok(group.ToDto());
    }

    public static IResult GetCurrent(string sessionToken)
    {
        User user = Session.Authenticate(sessionToken);

        if (user == null)
            return Results.Unauthorized();

        return Results.Ok(Group.GetByUser(user).Select(group => group.ToDto()));
    }

    public static IResult JoinFromInvite(string inviteCode, string sessionToken)
    {
        User user = Session.Authenticate(sessionToken);

        if (user == null)
            return Results.Unauthorized();

        Group group = Group.GetByInviteCode(inviteCode);

        if (group == null)
            return Results.NotFound();

        // (For now) only allow user to be part of one group
        if (Group.AnyGroupContainsUser(user))
            return Results.Conflict();

        group.AddUser(user);
        return Results.Ok();
    }
}

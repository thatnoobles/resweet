using System;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Resweet;
using Resweet.Api.Controllers;
using Resweet.Api.Utils;
using Resweet.Database.Utils;

internal class Program
{
    private static void Main()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();
        Configuration.Initialize();
        DatabaseUtils.Connect();

        app.MapGet("/", () => "Hello World!");

        // USER
        app.MapPost(
            "/user",
            ([FromBody] JsonElement json) => UserController.Create(json.ToJsonObject())
        );
        app.MapGet(
            "/user",
            (HttpRequest request) =>
            {
                if (!request.Headers.ContainsKey(SESSION))
                    return Results.Unauthorized();

                return UserController.GetCurrent(request.GetSession());
            }
        );
        app.MapGet("/user/{handle}", UserController.GetByHandle);

        // AUTH
        app.MapPost(
            "/auth/login",
            ([FromBody] JsonElement json) => AuthController.Login(json.ToJsonObject())
        );
        app.MapPost(
            "/auth/logout",
            (HttpRequest request) =>
            {
                if (!request.Headers.ContainsKey(SESSION))
                    return Results.Unauthorized();

                return AuthController.Logout(request.GetSession());
            }
        );

        // GROUP
        app.MapPost(
            "/group",
            (HttpRequest request, [FromBody] JsonElement json) =>
            {
                if (!request.Headers.ContainsKey(SESSION))
                    return Results.Unauthorized();

                return GroupController.Create(json.ToJsonObject(), request.GetSession());
            }
        );
        app.MapGet(
            "/group",
            (HttpRequest request) =>
            {
                if (!request.Headers.ContainsKey(SESSION))
                    return Results.Unauthorized();

                return GroupController.GetCurrent(request.GetSession());
            }
        );
        app.MapPost(
            "/join/{inviteCode}",
            (HttpRequest request, string inviteCode) =>
            {
                if (!request.Headers.ContainsKey(SESSION))
                    return Results.Unauthorized();

                return GroupController.JoinFromInvite(inviteCode, request.GetSession());
            }
        );

        // RECEIPT
        app.MapPost(
            "/receipt",
            (HttpRequest request, [FromBody] JsonElement json) =>
            {
                if (!request.Headers.ContainsKey(SESSION))
                    return Results.Unauthorized();

                return ReceiptController.Create(json.ToJsonObject(), request.GetSession());
            }
        );
        app.MapGet(
            "/receipt/{id}",
            (HttpRequest request, Guid id) =>
            {
                if (!request.Headers.ContainsKey(SESSION))
                    return Results.Unauthorized();

                return ReceiptController.GetById(id, request.GetSession());
            }
        );
        app.MapPut(
            "/receipt/{id}",
            (HttpRequest request, [FromBody] JsonElement json, Guid id) =>
            {
                if (!request.Headers.ContainsKey(SESSION))
                    return Results.Unauthorized();

                return ReceiptController.EditReceipt(id, json.ToJsonObject(), request.GetSession());
            }
        );
        app.MapPut(
            "/receipt/item/{id}",
            (HttpRequest request, [FromBody] JsonElement json, Guid id) =>
            {
                if (!request.Headers.ContainsKey(SESSION))
                    return Results.Unauthorized();

                return ReceiptController.EditReceiptItem(
                    id,
                    json.ToJsonObject(),
                    request.GetSession()
                );
            }
        );

        app.Run();
    }
}

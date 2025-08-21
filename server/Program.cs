using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Resweet;
using Resweet.Api.Controllers;
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
        app.MapPost("/user", ([FromBody] JsonElement json) => UserController.Create(json));
        app.MapGet(
            "/user",
            (HttpRequest request) =>
            {
                if (!request.Headers.ContainsKey("session"))
                    return Results.Unauthorized();

                return UserController.GetCurrent(request.Headers["session"]);
            }
        );
        app.MapGet("/user/{handle}", UserController.GetByHandle);

        // AUTH
        app.MapPost("/auth/login", ([FromBody] JsonElement json) => AuthController.Login(json));
        app.MapPost(
            "/auth/logout",
            (HttpRequest request) =>
            {
                if (!request.Headers.ContainsKey("session"))
                    return Results.Unauthorized();

                return AuthController.Logout(request.Headers["session"]);
            }
        );

        app.Run();
    }
}

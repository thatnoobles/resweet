using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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
        app.MapPost("/users", ([FromBody] JsonElement json) => UserController.Create(json));
        app.MapGet("/users/{handle}", UserController.GetByHandle);

        app.Run();
    }
}

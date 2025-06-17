using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Resweet.Api;
using Resweet.Database;

namespace Resweet;

internal class Program
{
    private static void Main()
    {
        Dictionary<string, string> config = File
            .ReadAllText("config.yml")
            .Split("\n")
            .ToDictionary(x => x.Split(":")[0].Trim(), x => x.Split(":")[1].Trim());

        DatabaseUtils.Initialize(
            config["host"],
            config["username"],
            config["password"],
            config["database"]);
        StartServer();
    }

    private static void StartServer()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // /receipt-items
        app.MapPost("/receipt-items", ([FromBody] JsonElement body) => ReceiptItemApi.CreateReceiptItem(
            body.GetProperty("name").GetString(),
            body.GetProperty("price").GetDouble()
        ));
        app.MapGet("/receipt-items", (string id = "") => ReceiptItemApi.GetReceiptItems(id));
        app.MapPut("/receipt-items", ([FromBody] JsonElement body) => ReceiptItemApi.UpdateReceiptItem(
            body.GetProperty("id").GetString(),
            body.GetProperty("name").GetString(),
            body.GetProperty("price").GetDouble()
        ));

        app.Run();
    }
}
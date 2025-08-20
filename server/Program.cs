using Microsoft.AspNetCore.Builder;

internal class Program
{
    private static void Main()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run();
    }
}
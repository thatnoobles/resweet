using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;

namespace Resweet.Api.Utils;

public static class ApiExtensions
{
    public static bool ContainsKeys(this JsonObject obj, params string[] keys)
    {
        foreach (string key in keys)
            if (!obj.ContainsKey(key))
                return false;

        return true;
    }

    public static JsonObject ToJsonObject(this JsonElement element) =>
        JsonNode.Parse(element.ToString()).AsObject();

    public static string GetSession(this HttpRequest request) => request.Headers[SESSION];
}

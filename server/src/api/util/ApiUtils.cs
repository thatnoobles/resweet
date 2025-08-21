using System.Collections.Generic;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Resweet.Api.Utils;

public static class ApiUtils
{
    public static bool IsPayloadValid(JsonElement obj, params string[] requiredKeys)
    {
        foreach (string key in requiredKeys)
            if (!obj.TryGetProperty(key, out _))
                return false;

        return true;
    }
}

using Resweet.Database;

namespace Resweet.Api;

public static class ApiUtils
{
    public static string ArrayToJson(Entity[] array)
    {
        string result = "[";

        for (int i = 0; i < array.Length; i++)
        {
            result += array[i].ToJson();

            if (i < array.Length - 1)
                result += ",";
        }

        result += "]";
        return result;
    }
}
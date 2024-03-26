using System.Text.RegularExpressions;

public class DataFunctions
{
    public static string camelToSnakeCase(string str)
    {
        return Regex.Replace(str, "(?<!^)([A-Z])", "_$1").ToLower();
    }
}

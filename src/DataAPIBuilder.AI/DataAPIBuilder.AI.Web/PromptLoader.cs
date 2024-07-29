namespace DataAPIBuilder.AI;

public class PromptLoader
{
    private static string PromptRepositoryPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Prompts");
    private static readonly Dictionary<string, string> _cache = [];

    public static string Load(string promptFileName)
    {
        if (!_cache.TryGetValue(promptFileName, out string? value))
        {
            value = Read(promptFileName);
            _cache[promptFileName] = value;
        }

        return value; ;
    }

    private static string Read(string promptFileName)
    {
        using Stream s = ReadStream(promptFileName);
        using var reader = new StreamReader(s);
        return reader.ReadToEnd();
    }

    private static Stream ReadStream(string promptFileName)
    {
        string path = Path.Combine(PromptRepositoryPath, promptFileName);
        if (!File.Exists(path)) { throw new Exception($"Prompt file not found: {path}"); }

        Stream s = new FileStream(path: path, mode: FileMode.Open, access: FileAccess.Read);
        return s;
    }
}
using Newtonsoft.Json;

namespace VoxCake.JsonBaker.Tests;

public class TestUtility
{
    private const string Ref = "C:/github/json-converter-generator/VoxCake.JsonBaker.Tests/bin/Debug/net6.0";

    public static T GetJson<T>(object testObject, string jsonFileName)
    {
        var jsonString = GetJson(testObject, jsonFileName);
        
        return JsonConvert.DeserializeObject<T>(jsonString);
    }
    
    public static string GetJson(object testObject, string jsonFileName)
    {
        Assert.IsTrue(jsonFileName.EndsWith(".json"));
        
        var currentDirectory = Directory.GetCurrentDirectory().Replace('\\', '/');
        var projectPath = currentDirectory.Replace("/bin/Debug/net6.0", "");

        var projectFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories)
            .Select(file => file.Replace("\\", "/"));

        var targetFileName = testObject.GetType().Name + ".cs";
        
        var folder = projectFiles.First(file => file.EndsWith(targetFileName))
            .Replace($"/{targetFileName}", "/Data");

        var jsonPath = Path.Combine(folder, jsonFileName).Replace("\\", "/");
        
        return File.ReadAllText(jsonPath);
    }
}
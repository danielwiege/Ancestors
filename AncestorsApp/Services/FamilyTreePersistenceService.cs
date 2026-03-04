using System.Text.Json;

namespace AncestorsApp.Services;

public class FamilyTreePersistenceService
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public void SaveToFile(FamilyTreeDocument document, string path)
    {
        var json = JsonSerializer.Serialize(document, SerializerOptions);
        File.WriteAllText(path, json);
    }

    public FamilyTreeDocument? LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<FamilyTreeDocument>(json);
    }
}

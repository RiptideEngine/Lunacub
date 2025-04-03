namespace Caxivitual.Lunacub.Tests;

public abstract class ResourcesFixture {
    public IReadOnlyDictionary<ResourceID, ResourceInfo> Options { get; }

    protected ResourcesFixture() {
        string resourceImport = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Resources.json"));
        
        Options = JsonSerializer.Deserialize<Dictionary<ResourceID, ResourceInfo>>(resourceImport, new JsonSerializerOptions(JsonSerializerOptions.Default) {
            Converters = {
                new JsonStringEnumConverter(),
            },
            TypeInfoResolver = new OptionsTypeInfoResolver(),
        })!;
    }

    public readonly record struct ResourceInfo(string Path, BuildingOptions Options);
}
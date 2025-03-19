namespace Caxivitual.Lunacub.Tests.Importing;

public partial class ImportEnvironmentTests : IDisposable {
    private readonly ImportEnvironment _env;
    private readonly ITestOutputHelper _output;
    
    private static readonly Guid _libraryGuid = Guid.NewGuid();

    public ImportEnvironmentTests(ITestOutputHelper output) {
        _output = output;
        AssertHelpers.RedirectConsoleOutput(output);

        MockFileSystem fs = new();
        
        _env = new();
        
        _env.Deserializers.Add(nameof(SimpleResourceDeserializer), new SimpleResourceDeserializer());
        _env.Deserializers.Add(nameof(ReferenceResourceDeserializer), new ReferenceResourceDeserializer());
        
        BuildEnvironment buildContext = new(new MockOutputSystem(fs));

        var simpleResourceRid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
        var refResourceRid1 = ResourceID.Parse("de1b416bf928467ea13bc0f23d3e6dfb");
        var refResourceRid2 = ResourceID.Parse("7a6646bd2ee446a1a91c884b76f12392");
        var refResourceRid3 = ResourceID.Parse("58f0c3e4c7c24f798129d45f248bfa2c");
        var refResourceRid4 = ResourceID.Parse("235be3d4ddfd42fa983ce7c9d9f58d51");
        
        buildContext.Resources.Add(simpleResourceRid, GetResourcePath("SimpleResource.json"), new() {
            ImporterName = nameof(SimpleResourceImporter),
            ProcessorName = null,
        });
        buildContext.Resources.Add(refResourceRid1, GetResourcePath("ReferenceResource1.json"), new() {
            ImporterName = nameof(ReferenceResourceImporter),
            ProcessorName = null,
        });
        buildContext.Resources.Add(refResourceRid2, GetResourcePath("ReferenceResource2.json"), new() {
            ImporterName = nameof(ReferenceResourceImporter),
            ProcessorName = null,
        });
        buildContext.Resources.Add(refResourceRid3, GetResourcePath("ReferenceResource3.json"), new() {
            ImporterName = nameof(ReferenceResourceImporter),
            ProcessorName = null,
        });
        buildContext.Resources.Add(refResourceRid4, GetResourcePath("ReferenceResource4.json"), new() {
            ImporterName = nameof(ReferenceResourceImporter),
            ProcessorName = null,
        });
        
        buildContext.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
        buildContext.Serializers.Add(new SimpleResourceSerializer());
        
        buildContext.Importers.Add(nameof(ReferenceResourceImporter), new ReferenceResourceImporter());
        buildContext.Serializers.Add(new ReferenceResourceSerializer());
        
        buildContext.BuildResources();

        MockResourceLibrary library = new(_libraryGuid, fs);
        _env.Input.Libraries.Add(library);

        return;
        
        static string GetResourcePath(string filename) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filename);
    }
    
    public void Dispose() {
        _env.Dispose();
    }
}
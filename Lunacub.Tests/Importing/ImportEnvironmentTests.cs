using Caxivitual.Lunacub.Compilation;
using System.IO.Abstractions.TestingHelpers;

namespace Caxivitual.Lunacub.Tests.Importing;

public sealed class ImportEnvironmentTests : IDisposable {
    private readonly ImportEnvironment _env;
    private readonly ITestOutputHelper _output;
    
    private static readonly Guid _libraryGuid = Guid.NewGuid();

    public ImportEnvironmentTests(ITestOutputHelper output) {
        _output = output;
        AssertHelpers.RedirectConsoleOutput(output);

        MockFileSystem fs = new();
        
        _env = new();
        
        _env.Deserializers.Add(nameof(SimpleResourceDeserializer), new SimpleResourceDeserializer());
        _env.Deserializers.Add(nameof(DependentResourceDeserializer), new DependentResourceDeserializer());
        _env.Deserializers.Add(nameof(CircularReferenceResourceDeserializer), new CircularReferenceResourceDeserializer());
        
        BuildEnvironment buildContext = new(new MockOutputSystem(fs));

        var simpleResourceRid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
        var dependentResourceRid = ResourceID.Parse("c5a8758032c94f2fa06c6ec22901f6e7");
        var dependencyResource1Rid = ResourceID.Parse("65609f8b1ae340769cfb6d4a38255fdc");
        var circularDependent1 = ResourceID.Parse("0195921b2ac17986b78abcb187f64dd2");
        var circularDependent2 = ResourceID.Parse("0195921aa48a7153906e0ecd9a7bdf33");
        
        buildContext.Resources.Add(simpleResourceRid, GetResourcePath("SimpleResource.json"), new() {
            ImporterName = nameof(SimpleResourceImporter),
            ProcessorName = null,
        });
        buildContext.Resources.Add(dependentResourceRid, GetResourcePath("DependentResource.json"), new() {
            ImporterName = nameof(DependentResourceImporter),
            ProcessorName = null,
        });
        buildContext.Resources.Add(dependencyResource1Rid, GetResourcePath("DependencyResource.json"), new() {
            ImporterName = nameof(SimpleResourceImporter),
            ProcessorName = null,
        });
        buildContext.Resources.Add(circularDependent1, GetResourcePath("CircularReferenceResource1.json"), new() {
            ImporterName = nameof(CircularReferenceResource),
            ProcessorName = null,
        });
        buildContext.Resources.Add(circularDependent2, GetResourcePath("CircularReferenceResource2.json"), new() {
            ImporterName = nameof(CircularReferenceResource),
            ProcessorName = null,
        });
        
        buildContext.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
        buildContext.Serializers.Add(new SimpleResourceSerializer());
        
        buildContext.Importers.Add(nameof(DependentResourceImporter), new DependentResourceImporter());
        buildContext.Serializers.Add(new DependentResourceSerializer());
        
        buildContext.Importers.Add(nameof(CircularReferenceResource), new CircularReferenceResourceImporter());
        buildContext.Serializers.Add(new CircularReferenceResourceSerializer());

        buildContext.BuildResources();

        MockResourceLibrary library = new(_libraryGuid, fs);
        _env.Input.Libraries.Add(library);

        return;
        
        static string GetResourcePath(string filename) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", filename);
    }
    
    public void Dispose() {
        _env.Dispose();
    }

    [Fact]
    public void SimpleResource_ShouldBeSuccess() {
        ResourceID rid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
        
        var fs = ((MockResourceLibrary)_env.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        
        new Func<object?>(() => _env.Import<object>(rid)).Should().NotThrow()
            .Which.Should().BeOfType<SimpleResource>()
            .Which.Value.Should().Be(69);
    }

    [Fact]
    public void DependentResource_ShouldBeSuccess() {
        ResourceID rid = ResourceID.Parse("c5a8758032c94f2fa06c6ec22901f6e7");
        
        var fs = ((MockResourceLibrary)_env.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        
        var dependent = new Func<object?>(() => _env.Import<object>(rid)).Should().NotThrow()
            .Which.Should().BeOfType<DependentResource>().Which;

        dependent.Dependency1.Should().NotBeNull();
        dependent.Dependency1!.Value.Should().Be(69);

        dependent.Dependency2.Should().BeNull();
    }

    [Fact]
    public void CircularDependentResource_ShouldBeSuccess() {
        ResourceID rid = ResourceID.Parse("0195921b2ac17986b78abcb187f64dd2");
        
        var fs = ((MockResourceLibrary)_env.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        
        var cd1 = new Func<object?>(() => _env.Import<object>(ResourceID.Parse("0195921b2ac17986b78abcb187f64dd2"))).Should().NotThrow()
            .Which.Should().BeOfType<CircularReferenceResource>().Which;
        
        cd1.Value.Should().Be(69);
        cd1.Reference.Should().NotBeNull();
        cd1.Reference!.Value.Should().Be(420);
    }
}
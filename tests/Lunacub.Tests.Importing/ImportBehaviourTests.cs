namespace Caxivitual.Lunacub.Tests.Importing;

// [Collection<PrebuildResourcesCollectionFixture>]

public class ImportBehaviourTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly ImportEnvironment _importEnvironment;

    public ImportBehaviourTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
        var buildSourceProvider = new BuildMemorySourceProvider();
        buildSourceProvider.Sources.Add(nameof(SimpleResource), new([.."{\"Value\":1}"u8], DateTime.MinValue));
        
        MemoryOutputSystem _buildOutput = new();

        using BuildEnvironment buildEnv = new BuildEnvironment(_buildOutput, new())
            .AddLibrary(
                new BuildResourceLibrary(1, buildSourceProvider).AddRegistryElement(1, new("SimpleResource", [], new() {
                    Addresses = new(nameof(SimpleResource)),
                    Options = new(nameof(SimpleResourceImporter)),
                }))
            );

        componentsFixture.ApplyComponents(buildEnv);
        buildEnv.BuildResources();
        
        var importSourceProvider = new ImportMemorySourceProvider();
        var importLibrary = new ImportResourceLibrary(1, importSourceProvider);
        
        foreach ((var resourceId, var compiledBinary) in _buildOutput.Outputs[1].CompiledResources) {
            importSourceProvider.Resources.Add(resourceId, compiledBinary.Item1);
        }
        
        foreach ((var resourceId, var registryElement) in _buildOutput.Outputs[1].Registry) {
            importLibrary.AddRegistryElement(resourceId, registryElement);
        }
        
        _importEnvironment = new ImportEnvironment()
            .SetLogger(output.BuildLogger())
            .AddLibrary(importLibrary);
        
        componentsFixture.ApplyComponents(_importEnvironment);
    }
    
    public void Dispose() {
        _importEnvironment.Dispose();
        GC.SuppressFinalize(this);
    }
    
    [Fact]
    public async Task DanglingResource_ImportFromAddress_ReturnsCorrectlyAndIncrementsReference() {
        var operation = _importEnvironment.Import(1, 1);

        await new Func<Task<ResourceHandle>>(() => operation.Task).Should().NotThrowAsync();

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);

        _importEnvironment.Libraries[0].Registry.Remove(1).Should().BeTrue();
        
        var operation2 = _importEnvironment.Import(1, 1);

        operation2.UnderlyingContainer.Should().Be(operation.UnderlyingContainer);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
    }
    
    [Fact]
    public async Task DanglingResource_ImportFromName_ReturnsCorrectlyAndIncrementsReference() {
        var operation = _importEnvironment.Import(1, $"{nameof(SimpleResource)}");

        await new Func<Task<ResourceHandle>>(() => operation.Task).Should().NotThrowAsync();

        operation.UnderlyingContainer.ReferenceCount.Should().Be(1);

        _importEnvironment.Libraries[0].Registry.Remove(1).Should().BeTrue();
        
        var operation2 = _importEnvironment.Import(1, 1);

        operation2.UnderlyingContainer.Should().Be(operation.UnderlyingContainer);

        operation.UnderlyingContainer.ReferenceCount.Should().Be(2);
    }
}
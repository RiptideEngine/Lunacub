namespace Caxivitual.Lunacub.Tests.Importing;

public class ImportOutputTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly ComponentsFixture _componentsFixture;

    private readonly MockFileSystem _fileSystem;
    private readonly BuildEnvironment _buildEnvironment;
    private readonly ImportEnvironment _importEnvironment;
    
    public ImportOutputTests(ComponentsFixture componentsFixture) {
        _componentsFixture = componentsFixture;

        _fileSystem = new();
        
        _buildEnvironment = new(new MockOutputSystem(_fileSystem));
        _importEnvironment = new();
        
        _componentsFixture.ApplyComponents(_buildEnvironment);
        _componentsFixture.ApplyComponents(_importEnvironment);
    }
    
    public void Dispose() {
        _importEnvironment.Dispose();
        _buildEnvironment.Dispose();
        
        GC.SuppressFinalize(this);
    }
    
    [Fact]
    public async Task ImportSimpleResource_ReturnsCorrectObject() {
        _buildEnvironment.Resources.Add(1, new("Resource", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("""{"Value":69}""", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(SimpleResourceImporter),
            },
        }));

        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new MockResourceLibrary(_fileSystem));

        var handle = (await new Func<Task<ResourceHandle<object>>>(() => _importEnvironment.Import<object>(1).Task)
            .Should()
            .CompleteWithinAsync(0.5.Seconds(), "shouldn't take this long."))
            .Which;

        handle.ResourceId.Should().Be((ResourceID)1);
        handle.Value.Should().NotBeNull().And.BeOfType<SimpleResource>().Which.Value.Should().Be(69);
    }
    
    [Fact]
    public async Task ImportConfigurableResource_WithBinaryOption_ReturnsCorrectObject() {
        _buildEnvironment.Resources.Add(1, new("Resource", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("[0,1,2,3,4]", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ConfigurableResourceImporter),
                Options = new ConfigurableResourceDTO.Options(OutputType.Binary),
            },
        }));

        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new MockResourceLibrary(_fileSystem));
        
        var handle = (await new Func<Task<ResourceHandle<object>>>(() => _importEnvironment.Import<object>(1).Task)
            .Should()
            .CompleteWithinAsync(0.5.Seconds(), "shouldn't take this long."))
            .Which;

        handle.ResourceId.Should().Be((ResourceID)1);
        handle.Value.Should().NotBeNull().And.BeOfType<ConfigurableResource>().Which.Array.Should().Equal(0, 1, 2, 3, 4);
    }
    
    [Fact]
    public async Task ImportConfigurableResource_WithJsonOption_ReturnsCorrectObject() {
        _buildEnvironment.Resources.Add(1, new("Resource", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("[0,1,2,3,4]", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ConfigurableResourceImporter),
                Options = new ConfigurableResourceDTO.Options(OutputType.Json),
            },
        }));

        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new MockResourceLibrary(_fileSystem));
        
        var handle = (await new Func<Task<ResourceHandle<object>>>(() => _importEnvironment.Import<object>(1).Task)
            .Should()
            .CompleteWithinAsync(0.5.Seconds(), "shouldn't take this long."))
            .Which;

        handle.ResourceId.Should().Be((ResourceID)1);
        handle.Value.Should().NotBeNull().And.BeOfType<ConfigurableResource>().Which.Array.Should().Equal(0, 1, 2, 3, 4);
    }

    [Fact]
    public async Task ImportReferencingResource_UnregisteredReference_ReturnsCorrectObjects() {
        _buildEnvironment.Resources.Add(1, new("A", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("""{"Value":1,"Reference":255}""", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferencingResourceImporter),
            },
        }));
        
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new MockResourceLibrary(_fileSystem));
        
        var handle = (await new Func<Task<ResourceHandle<object>>>(() => _importEnvironment.Import<object>(1).Task)
            .Should()
            .CompleteWithinAsync(0.5.Seconds(), "shouldn't take this long."))
            .Which;
        
        handle.ResourceId.Should().Be((ResourceID)1);
        var referencee = handle.Value.Should().NotBeNull().And.BeOfType<ReferencingResource>().Which;

        referencee.Value.Should().Be(1);
        referencee.Reference.Should().BeNull();
    }
    
    [Fact]
    public async Task ImportReferencingResource_Normal_ReturnsCorrectObjects() {
        _buildEnvironment.Resources.Add(1, new("A", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("""{"Value":1,"Reference":2}""", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferencingResourceImporter),
            },
        }));
        _buildEnvironment.Resources.Add(2, new("B", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("""{"Value":2,"Reference":0}""", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferencingResourceImporter),
            },
        }));
        
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new MockResourceLibrary(_fileSystem));
        
        var handle = (await new Func<Task<ResourceHandle<object>>>(() => _importEnvironment.Import<object>(1).Task)
            .Should()
            .CompleteWithinAsync(0.5.Seconds(), "shouldn't take this long."))
            .Which;
        
        handle.ResourceId.Should().Be((ResourceID)1);
        var resource1 = handle.Value.Should().NotBeNull().And.BeOfType<ReferencingResource>().Which;
        resource1.Value.Should().Be(1);

        var resource2 = resource1.Reference;
        resource2.Should().NotBeNull();
        resource2!.Value.Should().Be(2);
        resource2.Reference.Should().BeNull();
    }
    
    [Fact]
    public async Task ImportReferencingResource_Chain4_ReturnsCorrectObjects() {
        _buildEnvironment.Resources.Add(1, new("A", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("""{"Value":1,"Reference":2}""", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferencingResourceImporter),
            },
        }));
        _buildEnvironment.Resources.Add(2, new("B", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("""{"Value":2,"Reference":3}""", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferencingResourceImporter),
            },
        }));
        _buildEnvironment.Resources.Add(3, new("C", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("""{"Value":3,"Reference":4}""", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferencingResourceImporter),
            },
        }));
        _buildEnvironment.Resources.Add(4, new("D", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("""{"Value":4,"Reference":0}""", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ReferencingResourceImporter),
            },
        }));
        
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new MockResourceLibrary(_fileSystem));
        
        var handle = (await new Func<Task<ResourceHandle<object>>>(() => _importEnvironment.Import<object>(1).Task)
            .Should()
            .CompleteWithinAsync(0.5.Seconds(), "shouldn't take this long."))
            .Which;
        
        handle.ResourceId.Should().Be((ResourceID)1);
        var resource1 = handle.Value.Should().NotBeNull().And.BeOfType<ReferencingResource>().Which;
        resource1.Value.Should().Be(1);
        
        var resource2 = resource1.Reference;
        resource2.Should().NotBeNull();
        resource2!.Value.Should().Be(2);
        resource2.Reference.Should().NotBeNull();

        var resource3 = resource2.Reference;
        resource3.Should().NotBeNull();
        resource3!.Value.Should().Be(3);
        resource3.Reference.Should().NotBeNull();
        
        var resource4 = resource3.Reference;
        resource4.Should().NotBeNull();
        resource4!.Value.Should().Be(4);
        resource4.Reference.Should().BeNull();
    }

    [Fact]
    public async Task ImportTreeReferencingResource_Normal_ReturnsCorrectObjects() {
        
    }
}
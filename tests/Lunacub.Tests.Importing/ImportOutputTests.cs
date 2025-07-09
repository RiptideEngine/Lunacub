using System.Text.Json;
using MemorySourceProvider = Caxivitual.Lunacub.Building.Core.MemorySourceProvider;

namespace Caxivitual.Lunacub.Tests.Importing;

public class ImportOutputTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly ComponentsFixture _componentsFixture;
    private readonly ITestOutputHelper _output;

    private readonly MockFileSystem _fileSystem;
    private readonly BuildEnvironment _buildEnvironment;
    private readonly ImportEnvironment _importEnvironment;
    
    public ImportOutputTests(ComponentsFixture componentsFixture, ITestOutputHelper output) {
        _componentsFixture = componentsFixture;
        _output = output;
        
        _fileSystem = new();
        
        _buildEnvironment = new(new MockOutputSystem(_fileSystem));
        _importEnvironment = new() {
            Logger = _output.BuildLogger(),
        };
        
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
        _buildEnvironment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("""{"Value":69}""", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(SimpleResourceImporter)),
                }),
            },
        });
    
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new(new MockImportSourceProvider(_fileSystem)) {
            Registry = {
                [1] = new("Resource", [], 0)
            }
        });
    
        var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(1).Task)
            .Should()
            .NotThrowAsync())
            .Which;
    
        handle.ResourceId.Should().Be((ResourceID)1);
        handle.Value.Should().NotBeNull().And.BeOfType<SimpleResource>().Which.Value.Should().Be(69);
    }
    
    [Fact]
    public async Task ImportConfigurableResource_WithBinaryOption_ReturnsCorrectObject() {
        _buildEnvironment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("[0,1,2,3,4]", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(ConfigurableResourceImporter), null, new ConfigurableResourceDTO.Options(OutputType.Binary)),
                }),
            },
        });
    
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new(new MockImportSourceProvider(_fileSystem)){
            Registry = {
                [1] = new("Resource", [], 0)
            }
        });
        
        var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(1).Task)
            .Should()
            .NotThrowAsync())
            .Which;
    
        handle.ResourceId.Should().Be((ResourceID)1);
        handle.Value.Should().NotBeNull().And.BeOfType<ConfigurableResource>().Which.Array.Should().Equal(0, 1, 2, 3, 4);
    }
    
    [Fact]
    public async Task ImportConfigurableResource_WithJsonOption_ReturnsCorrectObject() {
        _buildEnvironment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("[0,1,2,3,4]", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(ConfigurableResourceImporter), null, new ConfigurableResourceDTO.Options(OutputType.Json)),
                }),
            },
        });
    
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new(new MockImportSourceProvider(_fileSystem)){
            Registry = {
                [1] = new("Resource", [], 0)
            }
        });
        
        var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(1).Task)
            .Should()
            .NotThrowAsync())
            .Which;
    
        handle.ResourceId.Should().Be((ResourceID)1);
        handle.Value.Should().NotBeNull().And.BeOfType<ConfigurableResource>().Which.Array.Should().Equal(0, 1, 2, 3, 4);
    }
    
    [Fact]
    public async Task ImportReferencingResource_UnregisteredReference_ReturnsCorrectObjects() {
        _buildEnvironment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("""{"Reference":2,"Value":1}""", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(ReferencingResourceImporter)),
                }),
            },
        });
        
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new(new MockImportSourceProvider(_fileSystem)){
            Registry = {
                [1] = new("Resource", [], 0)
            }
        });
        
        var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(1).Task)
            .Should()
            .NotThrowAsync())
            .Which;
        
        handle.ResourceId.Should().Be((ResourceID)1);
        var referencee = handle.Value.Should().NotBeNull().And.BeOfType<ReferencingResource>().Which;
    
        referencee.Value.Should().Be(1);
        referencee.Reference.Should().BeNull();
    }
    
    [Fact]
    public async Task ImportReferencingResource_Normal_ReturnsCorrectObjects() {
        _buildEnvironment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("""{"Reference":2,"Value":1}""", DateTime.MinValue),
                ["Reference"] = MemorySourceProvider.AsUtf8("""{"Value":2}""", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(ReferencingResourceImporter)),
                }),
                [2] = new("Reference", [], new() {
                    Addresses = new("Reference"),
                    Options = new(nameof(ReferencingResourceImporter))
                })
            },
        });
        
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new(new MockImportSourceProvider(_fileSystem)) {
            Registry = {
                [1] = new("Resource", [], 0),
                [2] = new("Reference", [], 0),
            }
        });
        
        var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(1).Task)
            .Should()
            .NotThrowAsync())
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
        _buildEnvironment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource1"] = MemorySourceProvider.AsUtf8("""{"Reference":2,"Value":1}""", DateTime.MinValue),
                ["Resource2"] = MemorySourceProvider.AsUtf8("""{"Reference":3,"Value":2}""", DateTime.MinValue),
                ["Resource3"] = MemorySourceProvider.AsUtf8("""{"Reference":4,"Value":3}""", DateTime.MinValue),
                ["Resource4"] = MemorySourceProvider.AsUtf8("""{"Value":4}""", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource1", [], new() {
                    Addresses = new("Resource1"),
                    Options = new(nameof(ReferencingResourceImporter)),
                }),
                [2] = new("Resource2", [], new() {
                    Addresses = new("Resource2"),
                    Options = new(nameof(ReferencingResourceImporter)),
                }),
                [3] = new("Resource3", [], new() {
                    Addresses = new("Resource3"),
                    Options = new(nameof(ReferencingResourceImporter)),
                }),
                [4] = new("Resource4", [], new() {
                    Addresses = new("Resource4"),
                    Options = new(nameof(ReferencingResourceImporter)),
                }),
            },
        });
        
        _buildEnvironment.BuildResources();
        _importEnvironment.Libraries.Add(new(new MockImportSourceProvider(_fileSystem)) {
            Registry = {
                [1] = new("Resource1", [], 0),
                [2] = new("Resource2", [], 0),
                [3] = new("Resource3", [], 0),
                [4] = new("Resource4", [], 0),
            },
        });
        
        var handle = (await new Func<Task<ResourceHandle>>(() => _importEnvironment.Import(1).Task)
            .Should()
            .NotThrowAsync())
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
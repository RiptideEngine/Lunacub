// ReSharper disable AccessToDisposedClosure

using System.Collections.Immutable;

namespace Caxivitual.Lunacub.Tests.Building;

public sealed class BinaryOutputTests : IClassFixture<ComponentsFixture>, IClassFixture<MemoryStreamManagerFixture>, IDisposable {
    private readonly MemoryResourceSink _resourceSink;
    private readonly BuildEnvironment _environment;

    public BinaryOutputTests(ComponentsFixture componentsFixture, MemoryStreamManagerFixture memoryStreamFixture) {
        _environment = new(_resourceSink = new(), VoidBuildCacheSink.Instance, memoryStreamFixture.Manager);
        
        componentsFixture.ApplyComponents(_environment);
    }
    
    [Fact]
    public void BuildSimpleResource_OutputCorrectBinary() {
        _environment.Libraries.Add(new(1, new MemorySourceRepository {
            Sources = {
                ["Resource"] = MemorySourceRepository.AsUtf8("""{"Value":255}""", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(SimpleResourceImporter)),
                }),
            },
        });

        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.FailureResults.Should().BeEmpty();

        ImmutableArray<byte> compiledBinary = new Func<ImmutableArray<byte>>(() => _resourceSink.Outputs[1].CompiledResources[1]).Should().NotThrow().Which;
        Header layout = new Func<Header>(() => Header.Extract(compiledBinary.AsSpan())).Should().NotThrow().Which;

        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var dataChunkInfo).Should().BeTrue();
        dataChunkInfo.Length.Should().Be(4);

        compiledBinary.Slice((int)dataChunkInfo.ContentOffset, 4).Should().Equal(BitConverter.GetBytes(255));
    }
    
    [Fact]
    public unsafe void BuildReferenceResource_OutputCorrectBinary() {
        _environment.Libraries.Add(new(1, new MemorySourceRepository {
            Sources = {
                ["Resource"] = MemorySourceRepository.AsUtf8("""{"ReferenceAddress":{"LibraryId":1,"ResourceId":2},"Value":50}""", DateTime.MinValue),
                ["Reference"] = MemorySourceRepository.AsUtf8("""{"Value":100}""", DateTime.MinValue),
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

        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.FailureResults.Should().BeEmpty();

        ImmutableArray<byte> compiledBinary = new Func<ImmutableArray<byte>>(() => _resourceSink.Outputs[1].CompiledResources[1]).Should().NotThrow().Which;
        Header layout = new Func<Header>(() => Header.Extract(compiledBinary.AsSpan())).Should().NotThrow().Which;

        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var dataChunkInfo).Should().BeTrue();
        dataChunkInfo.Length.Should().Be((uint)(sizeof(ResourceAddress) + sizeof(int)));

        compiledBinary.Slice((int)dataChunkInfo.ContentOffset, sizeof(ResourceAddress) + sizeof(int)).Should().Equal([
            ..BitConverter.GetBytes(new LibraryID(1).Value),
            ..BitConverter.GetBytes(new ResourceID(2).Value),
            ..BitConverter.GetBytes(50),
        ]);

        compiledBinary = new Func<ImmutableArray<byte>>(() => _resourceSink.Outputs[1].CompiledResources[2]).Should().NotThrow().Which;
        layout = new Func<Header>(() => Header.Extract(compiledBinary.AsSpan())).Should().NotThrow().Which;

        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out dataChunkInfo).Should().BeTrue();
        dataChunkInfo.Length.Should().Be((uint)(sizeof(ResourceAddress) + sizeof(int)));

        compiledBinary.Slice((int)dataChunkInfo.ContentOffset, sizeof(ResourceAddress) + sizeof(int)).Should().Equal([
            ..BitConverter.GetBytes(new LibraryID(0).Value),
            ..BitConverter.GetBytes(new ResourceID(0).Value),
            ..BitConverter.GetBytes(100),
        ]);
    }
    
    [Fact]
    public void BuildConfigurableResource_Json_OutputCorrectBinary() {
        _environment.Libraries.Add(new(1, new MemorySourceRepository {
            Sources = {
                ["Resource"] = MemorySourceRepository.AsUtf8("[1,2,3,4,5]", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(ConfigurableResourceImporter), null, new ConfigurableResourceDTO.Options(OutputType.Json)),
                }),
            },
        });

        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.FailureResults.Should().BeEmpty();

        ImmutableArray<byte> compiledBinary = new Func<ImmutableArray<byte>>(() => _resourceSink.Outputs[1].CompiledResources[1]).Should().NotThrow().Which;
        Header layout = new Func<Header>(() => Header.Extract(compiledBinary.AsSpan())).Should().NotThrow().Which;

        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var dataChunkInfo).Should().BeTrue();
        dataChunkInfo.Length.Should().Be(11);

        compiledBinary.Slice((int)dataChunkInfo.ContentOffset, 11).Should().Equal("[1,2,3,4,5]"u8.ToArray());
    }
    
    [Fact]
    public void BuildConfigurableResource_Binary_OutputCorrectBinary() {
        _environment.Libraries.Add(new(1, new MemorySourceRepository {
            Sources = {
                ["Resource"] = MemorySourceRepository.AsUtf8("[1,2,3,4,5]", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(ConfigurableResourceImporter), null, new ConfigurableResourceDTO.Options(OutputType.Binary)),
                }),
            },
        });

        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.FailureResults.Should().BeEmpty();

        ImmutableArray<byte> compiledBinary = new Func<ImmutableArray<byte>>(() => _resourceSink.Outputs[1].CompiledResources[1]).Should().NotThrow().Which;
        Header layout = new Func<Header>(() => Header.Extract(compiledBinary.AsSpan())).Should().NotThrow().Which;

        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var dataChunkInfo).Should().BeTrue();
        dataChunkInfo.Length.Should().Be(20);

        compiledBinary.Slice((int)dataChunkInfo.ContentOffset, 20).Should().Equal([
            ..BitConverter.GetBytes(1),
            ..BitConverter.GetBytes(2),
            ..BitConverter.GetBytes(3),
            ..BitConverter.GetBytes(4),
            ..BitConverter.GetBytes(5),
        ]);
    }

    public void Dispose() {
        _environment.Dispose();
        GC.SuppressFinalize(this);
    }

    ~BinaryOutputTests() {
        _environment.Dispose();
    }
}
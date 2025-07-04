﻿// ReSharper disable AccessToDisposedClosure

using Caxivitual.Lunacub.Extensions;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Caxivitual.Lunacub.Tests.Building;

public sealed class BinaryOutputTests : IClassFixture<ComponentsFixture>, IDisposable {
    private readonly BuildEnvironment _environment;

    public BinaryOutputTests(ComponentsFixture componentsFixture) {
        _environment = new(new MockOutputSystem());
        
        componentsFixture.ApplyComponents(_environment);
    }
    
    [Fact]
    public void BuildSimpleResource_OutputCorrectBinary() {
        _environment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("""{"Value":255}""", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(SimpleResourceImporter)),
                }),
            },
        });
        
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.ResourceResults.Should().ContainSingle();

        MockFileSystem fs = ((MockOutputSystem)_environment.Output).FileSystem;
        using Stream stream = new Func<Stream>(() => GetResourceBinaryStream(fs, 1)).Should().NotThrow().Which;
        BinaryHeader layout = new Func<BinaryHeader>(() => BinaryHeader.Extract(stream)).Should().NotThrow().Which;

        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var dataChunkInfo).Should().BeTrue();
        dataChunkInfo.Length.Should().Be(4);
        
        stream.Seek(dataChunkInfo.ContentOffset, SeekOrigin.Begin);

        using BinaryReader br = new(stream);

        br.ReadInt32().Should().Be(255);
    }
    
    [Fact]
    public unsafe void BuildReferenceResource_OutputCorrectBinary() {
        _environment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("""{"Reference":2,"Value":50}""", DateTime.MinValue),
                ["Reference"] = MemorySourceProvider.AsUtf8("""{"Reference":1,"Value":100}""", DateTime.MinValue),
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

        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.ResourceResults.Should().HaveCount(2);

        MockFileSystem fs = ((MockOutputSystem)_environment.Output).FileSystem;

        using (Stream stream = new Func<Stream>(() => GetResourceBinaryStream(fs, 1)).Should().NotThrow().Which) {
            BinaryHeader layout = new Func<BinaryHeader>(() => BinaryHeader.Extract(stream)).Should().NotThrow().Which;
            
            layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var dataChunkInfo).Should().BeTrue();
            dataChunkInfo.Length.Should().Be((uint)(sizeof(ResourceID) + sizeof(int)));

            stream.Seek(dataChunkInfo.ContentOffset, SeekOrigin.Begin);

            using (BinaryReader br = new(stream)) {
                br.ReadResourceID().Should().Be(new ResourceID(2));
                br.ReadInt32().Should().Be(50);
            }
        }
        
        using (Stream stream = new Func<Stream>(() => GetResourceBinaryStream(fs, 2)).Should().NotThrow().Which) {
            BinaryHeader layout = new Func<BinaryHeader>(() => BinaryHeader.Extract(stream)).Should().NotThrow().Which;
            
            layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var dataChunkInfo).Should().BeTrue();
            dataChunkInfo.Length.Should().Be((uint)(sizeof(ResourceID) + sizeof(int)));

            stream.Seek(dataChunkInfo.ContentOffset, SeekOrigin.Begin);

            using (BinaryReader br = new(stream)) {
                br.ReadResourceID().Should().Be(new ResourceID(1));
                br.ReadInt32().Should().Be(100);
            }
        }
    }
    
    [Fact]
    public void BuildConfigurableResource_Json_OutputCorrectBinary() {
        _environment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("[1,2,3,4,5]", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(ConfigurableResourceImporter), null, new ConfigurableResourceDTO.Options(OutputType.Json)),
                }),
            },
        });
        
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.ResourceResults.Should().ContainSingle();

        MockFileSystem fs = ((MockOutputSystem)_environment.Output).FileSystem;
        using (Stream stream = new Func<Stream>(() => GetResourceBinaryStream(fs, 1)).Should().NotThrow().Which) {
            BinaryHeader layout = new Func<BinaryHeader>(() => BinaryHeader.Extract(stream)).Should().NotThrow().Which;

            layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var dataChunkInfo).Should().BeTrue();
            dataChunkInfo.Length.Should().Be(11);

            byte[] buffer = new byte[dataChunkInfo.Length];
            stream.Seek(dataChunkInfo.ContentOffset, SeekOrigin.Begin);
            stream.ReadExactly(buffer);

            JsonSerializer.Deserialize<int[]>(buffer).Should().Equal(1, 2, 3, 4, 5);
        }
    }
    
    [Fact]
    public void BuildConfigurableResource_Binary_OutputCorrectBinary() {
        _environment.Libraries.Add(new(new MemorySourceProvider {
            Sources = {
                ["Resource"] = MemorySourceProvider.AsUtf8("[1,2,3,4,5]", DateTime.MinValue),
            },
        }) {
            Registry = {
                [1] = new("Resource", [], new() {
                    Addresses = new("Resource"),
                    Options = new(nameof(ConfigurableResourceImporter), null, new ConfigurableResourceDTO.Options(OutputType.Binary)),
                }),
            },
        });
        
        new Func<BuildingResult>(() => _environment.BuildResources()).Should().NotThrow().Which.ResourceResults.Should().ContainSingle();

        MockFileSystem fs = ((MockOutputSystem)_environment.Output).FileSystem;
        using (Stream stream = new Func<Stream>(() => GetResourceBinaryStream(fs, 1)).Should().NotThrow().Which) {
            BinaryHeader layout = new Func<BinaryHeader>(() => BinaryHeader.Extract(stream)).Should().NotThrow().Which;

            layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var dataChunkInfo).Should().BeTrue();
            dataChunkInfo.Length.Should().Be(20);

            stream.Seek(dataChunkInfo.ContentOffset, SeekOrigin.Begin);
            
            using BinaryReader br = new(stream);
            
            br.ReadInt32().Should().Be(1);
            br.ReadInt32().Should().Be(2);
            br.ReadInt32().Should().Be(3);
            br.ReadInt32().Should().Be(4);
            br.ReadInt32().Should().Be(5);
        }
    }

    private static Stream GetResourceBinaryStream(MockFileSystem fs, ResourceID rid) {
        return fs.File.OpenRead(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}"));
    }

    public void Dispose() {
        _environment.Dispose();
        GC.SuppressFinalize(this);
    }

    ~BinaryOutputTests() {
        _environment.Dispose();
    }
}
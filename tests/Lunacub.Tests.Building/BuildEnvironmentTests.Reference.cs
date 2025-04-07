// ReSharper disable AccessToDisposedClosure
namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public unsafe void BuildReferenceResource_Sufficient_BuildCorrectly() {
        ResourceID rid1 = new("de1b416bf928467ea13bc0f23d3e6dfb");
        ResourceID rid2 = new("7a6646bd2ee446a1a91c884b76f12392");
        
        _resourcesFixture.RegisterResourceToBuild(_env, rid1);
        _resourcesFixture.RegisterResourceToBuild(_env, rid2);
        
        MockFileSystem fs = ((MockOutputSystem)_env.Output).FileSystem;
        
        var results = new Func<BuildingResult>(() => _env.BuildResources()).Should().NotThrow().Which.ResourceResults;

        var result1 = results.Should().ContainKey(rid1).WhoseValue;
        var result2 = results.Should().ContainKey(rid2).WhoseValue;
        
        ((int)result1.Status).Should().BeGreaterThanOrEqualTo((int)BuildStatus.Success);
        ((int)result2.Status).Should().BeGreaterThanOrEqualTo((int)BuildStatus.Success);
        
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        
        var resource1Path = fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid1}{CompilingConstants.CompiledResourceExtension}");
        var resource2Path = fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid2}{CompilingConstants.CompiledResourceExtension}");

        using Stream resource1Stream = new Func<Stream>(() => fs.File.OpenRead(resource1Path)).Should().NotThrow().Which;
        var layout = new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(resource1Stream)).Should().NotThrow().Which;
        
        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var chunkInfo).Should().BeTrue();
        chunkInfo.Length.Should().Be((uint)sizeof(ResourceID) + sizeof(int));
        
        using Stream resource2Stream = new Func<Stream>(() => fs.File.OpenRead(resource2Path)).Should().NotThrow().Which;
        layout = new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(resource2Stream)).Should().NotThrow().Which;
        
        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out chunkInfo).Should().BeTrue();
        chunkInfo.Length.Should().Be((uint)sizeof(ResourceID) + sizeof(int));
    }
    
    [Fact]
    public unsafe void BuildReferenceResource_UnregisteredDependency_BuildCorrectly() {
        ResourceID rid1 = new("de1b416bf928467ea13bc0f23d3e6dfb");
        
        _resourcesFixture.RegisterResourceToBuild(_env, rid1);
        
        MockFileSystem fs = ((MockOutputSystem)_env.Output).FileSystem;
        
        var results = new Func<BuildingResult>(() => _env.BuildResources()).Should().NotThrow().Which.ResourceResults;

        var result1 = results.Should().ContainKey(rid1).WhoseValue;
        result1.IsSuccess.Should().BeTrue();
        
        var resource1Path = fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid1}{CompilingConstants.CompiledResourceExtension}");

        using Stream resource1Stream = new Func<Stream>(() => fs.File.OpenRead(resource1Path)).Should().NotThrow().Which;
        var layout = new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(resource1Stream)).Should().NotThrow().Which;
        
        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var chunkInfo).Should().BeTrue();
        chunkInfo.Length.Should().Be((uint)sizeof(ResourceID) + sizeof(int));
    }
}
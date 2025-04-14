// ReSharper disable AccessToDisposedClosure
namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void BuildOptionsResource_JsonOption_CompilesCorrectly() {
        ResourceID rid = new("81a12ccd19f15cd6a5df2513c95ffbd1");
        
        _resourcesFixture.RegisterResourceToBuild(_env, rid);
        
        MockFileSystem fs = ((MockOutputSystem)_env.Output).FileSystem;
        
        var result = new Func<BuildingResult>(() => _env.BuildResources()).Should().NotThrow().Which.
            ResourceResults.Should().ContainKey(rid).WhoseValue;
        
        result.Exception.Should().BeNull();
        ((int)result.Status).Should().BeGreaterThanOrEqualTo((int)BuildStatus.Success);
        result.IsSuccess.Should().BeTrue();
        
        string path = fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        using Stream stream = fs.File.OpenRead(path);
        var layout = new Func<CompiledResourceLayout>(() => LayoutExtracting.Extract(stream)).Should().NotThrow().Which;
        
        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var chunkInfo).Should().BeTrue();
        chunkInfo.Length.Should().Be((uint)"[0,1,2,3,4,5,6,7,8,9]".Length);
    }
    
    [Fact]
    public void BuildOptionsResource_BinaryOption_CompilesCorrectly() {
        ResourceID rid = new("178ad6eee6e4521f91c9668566a4b6eb");
        
        _resourcesFixture.RegisterResourceToBuild(_env, rid);
        
        MockFileSystem fs = ((MockOutputSystem)_env.Output).FileSystem;
        
        var result = new Func<BuildingResult>(() => _env.BuildResources()).Should().NotThrow().Which.
            ResourceResults.Should().ContainKey(rid).WhoseValue;
        
        result.Exception.Should().BeNull();
        ((int)result.Status).Should().BeGreaterThanOrEqualTo((int)BuildStatus.Success);
        result.IsSuccess.Should().BeTrue();
        
        string path = fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        using Stream stream = fs.File.OpenRead(path);
        var layout = new Func<CompiledResourceLayout>(() => LayoutExtracting.Extract(stream)).Should().NotThrow().Which;
        
        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var chunkInfo).Should().BeTrue();
        chunkInfo.Length.Should().Be(40);
    }
}
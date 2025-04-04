// ReSharper disable AccessToDisposedClosure

namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void SimpleResource_ShouldBeCorrect() {
        var rid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
        
        _resourcesFixture.RegisterResourceToBuild(_env, rid);
        
        MockFileSystem fs = ((MockOutputSystem)_env.Output).FileSystem;

        var result = new Func<BuildingResult>(() => _env.BuildResources()).Should().NotThrow().Which.
            ResourceResults.Should().ContainKey(rid).WhoseValue;

        ((int)result.Status).Should().BeGreaterThanOrEqualTo((int)BuildStatus.Success);
        result.Exception.Should().BeNull();
        result.IsSuccess.Should().BeTrue();

        string path = fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        fs.File.Exists(path).Should().BeTrue();

        using Stream stream = fs.File.OpenRead(path);
        CompiledResourceLayout layout = new Func<CompiledResourceLayout>(() => LayoutValidation.Validate(stream)).Should().NotThrow().Which;

        layout.TryGetChunkInformation(CompilingConstants.ResourceDataChunkTag, out var chunkInfo).Should().BeTrue();
        chunkInfo.Length.Should().Be(4);
    }
}
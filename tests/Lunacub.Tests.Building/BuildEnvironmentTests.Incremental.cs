namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void Build_Incremental_RebuildShouldBeCached() {
        ResourceID rid = new("e0b8066bf60043c5a0c3a7782363427d");
        
        _resourcesFixture.RegisterResourceToBuild(_env, rid);
        
        MockFileSystem fs = ((MockOutputSystem)_env.Output).FileSystem;

        var result = new Func<BuildingResult>(() => _env.BuildResources())
            .Should().NotThrow().Which.ResourceResults.Should().ContainSingle().Which;

        result.Key.Should().Be(rid);
        result.Value.Status.Should().Be(BuildStatus.Success);

        result = new Func<BuildingResult>(() => _env.BuildResources())
            .Should().NotThrow().Which.ResourceResults.Should().ContainSingle().Which;

        result.Key.Should().Be(rid);
        result.Value.Status.Should().Be(BuildStatus.Cached);
    }

    [Fact]
    public void Build_Incremental_RebuildAfterIncrementalInfoRemoved() {
        ResourceID rid = new("e0b8066bf60043c5a0c3a7782363427d");
        
        _resourcesFixture.RegisterResourceToBuild(_env, rid);
        
        MockFileSystem fs = ((MockOutputSystem)_env.Output).FileSystem;

        var result = new Func<BuildingResult>(() => _env.BuildResources())
            .Should().NotThrow().Which.ResourceResults.Should().ContainSingle().Which;

        result.Key.Should().Be(rid);
        result.Value.Status.Should().Be(BuildStatus.Success);

        _env.IncrementalInfos.Remove(rid).Should().BeTrue();

        result = new Func<BuildingResult>(() => _env.BuildResources())
            .Should().NotThrow().Which.ResourceResults.Should().ContainSingle().Which;

        result.Key.Should().Be(rid);
        result.Value.Status.Should().Be(BuildStatus.Success);
    }
}
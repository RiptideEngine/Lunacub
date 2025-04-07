namespace Caxivitual.Lunacub.Tests.Building;

partial class BuildEnvironmentTests {
    [Fact]
    public void Build_CoverageAll_ShouldBeCorrect() {
        _resourcesFixture.RegisterResourceToBuild(_env, new("e0b8066bf60043c5a0c3a7782363427d"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("de1b416bf928467ea13bc0f23d3e6dfb"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("7a6646bd2ee446a1a91c884b76f12392"));
        
        var result = new Func<BuildingResult>(() => _env.BuildResources()).Should().NotThrow().Which;
        result.ResourceResults.Should().HaveCount(3);
    }

    [Fact]
    public void Build_NoDependencyCoverage_BuildsSingleResource() {
        ResourceID rid = new("e0b8066bf60043c5a0c3a7782363427d");
        
        _resourcesFixture.RegisterResourceToBuild(_env, rid);
        _resourcesFixture.RegisterResourceToBuild(_env, new("de1b416bf928467ea13bc0f23d3e6dfb"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("7a6646bd2ee446a1a91c884b76f12392"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("58f0c3e4c7c24f798129d45f248bfa2c"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("235be3d4ddfd42fa983ce7c9d9f58d51"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("178ad6eee6e4521f91c9668566a4b6eb"));
        
        var result = new Func<BuildingResult>(() => _env.BuildResource(rid)).Should().NotThrow().Which;
        result.ResourceResults.Should().HaveCount(1).And.ContainKey(rid);
    }

    [Fact]
    public void Build_DependencyCoverage_BuildsWithDependencies() {
        ResourceID dependantRid = new("de1b416bf928467ea13bc0f23d3e6dfb");
        ResourceID dependencyRid = new("7a6646bd2ee446a1a91c884b76f12392");
        
        _resourcesFixture.RegisterResourceToBuild(_env, new("e0b8066bf60043c5a0c3a7782363427d"));
        _resourcesFixture.RegisterResourceToBuild(_env, dependantRid);
        _resourcesFixture.RegisterResourceToBuild(_env, new("7a6646bd2ee446a1a91c884b76f12392"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("58f0c3e4c7c24f798129d45f248bfa2c"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("235be3d4ddfd42fa983ce7c9d9f58d51"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("178ad6eee6e4521f91c9668566a4b6eb"));
        
        var result = new Func<BuildingResult>(() => _env.BuildResource(dependantRid)).Should().NotThrow().Which;
        result.ResourceResults.Should().HaveCount(2).And.ContainKey(dependantRid).And.ContainKey(dependencyRid);
    }
    
    [Fact]
    public void Build_UnregisteredDependencyCoverage_IgnoreUnregisteredDependencies() {
        ResourceID dependantRid = new("de1b416bf928467ea13bc0f23d3e6dfb");
        
        _resourcesFixture.RegisterResourceToBuild(_env, new("e0b8066bf60043c5a0c3a7782363427d"));
        _resourcesFixture.RegisterResourceToBuild(_env, dependantRid);
        _resourcesFixture.RegisterResourceToBuild(_env, new("58f0c3e4c7c24f798129d45f248bfa2c"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("235be3d4ddfd42fa983ce7c9d9f58d51"));
        _resourcesFixture.RegisterResourceToBuild(_env, new("178ad6eee6e4521f91c9668566a4b6eb"));
        
        var result = new Func<BuildingResult>(() => _env.BuildResource(dependantRid)).Should().NotThrow().Which;
        result.ResourceResults.Should().HaveCount(1).And.ContainKey(dependantRid);
    }
}
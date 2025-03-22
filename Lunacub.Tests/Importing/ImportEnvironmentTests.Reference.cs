namespace Caxivitual.Lunacub.Tests.Importing;

partial class ImportEnvironmentTests {
    [Fact]
    public void ReferenceResource_ShouldBeSuccess() {
        ResourceID rid1 = ResourceID.Parse("de1b416bf928467ea13bc0f23d3e6dfb");
        ResourceID rid2 = ResourceID.Parse("7a6646bd2ee446a1a91c884b76f12392");
        
        BuildResources(rid1, rid2);
        
        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        
        _fileSystem.File.Exists(_fileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid1}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        _fileSystem.File.Exists(_fileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid2}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        
        var ref1 = new Func<object?>(() => _importEnv.Import<object>(rid1)).Should().NotThrow().Which.Should().BeOfType<ReferenceResource>().Which;
        var ref2 = new Func<object?>(() => _importEnv.Import<object>(rid2)).Should().NotThrow().Which.Should().BeOfType<ReferenceResource>().Which;
            
        ref1.Value.Should().Be(69);
        ref2.Value.Should().Be(420);

        ref1.Reference.Should().BeSameAs(ref2);
        ref2.Reference.Should().BeNull();
    }
    
    [Fact]
    public void ReferenceResource_CircularReference_ShouldBeSuccess() {
        ResourceID rid3 = ResourceID.Parse("58f0c3e4c7c24f798129d45f248bfa2c");
        ResourceID rid4 = ResourceID.Parse("235be3d4ddfd42fa983ce7c9d9f58d51");
        
        BuildResources(rid3, rid4);
        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        
        _fileSystem.File.Exists(_fileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid3}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        _fileSystem.File.Exists(_fileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid4}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        
        var ref1 = new Func<object?>(() => _importEnv.Import<object>(rid3)).Should().NotThrow().Which.Should().BeOfType<ReferenceResource>().Which;
        var ref2 = new Func<object?>(() => _importEnv.Import<object>(rid4)).Should().NotThrow().Which.Should().BeOfType<ReferenceResource>().Which;
            
        ref1.Value.Should().Be(69);
        ref2.Value.Should().Be(420);

        ref1.Reference.Should().BeSameAs(ref2);
        ref2.Reference.Should().BeSameAs(ref1);
    }
}
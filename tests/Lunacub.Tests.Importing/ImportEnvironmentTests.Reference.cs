namespace Caxivitual.Lunacub.Tests.Importing;

partial class ImportEnvironmentTests {
    [Fact]
    public void ImportReferenceResource_Normal_ShouldHaveCorrectValueAndReference() {
        ResourceID rid1 = ResourceID.Parse("de1b416bf928467ea13bc0f23d3e6dfb");
        ResourceID rid2 = ResourceID.Parse("7a6646bd2ee446a1a91c884b76f12392");
        
        BuildResources(rid1, rid2);
        
        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        
        _fileSystem.File.Exists(_fileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid1}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        _fileSystem.File.Exists(_fileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid2}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        var handle = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid1)).Should().NotThrow().Which;
        handle.Rid.Should().Be(rid1);
        var resource1 = handle.Value.Should().BeOfType<ReferenceResource>().Which;
        resource1.Value.Should().Be(69);
        
        handle = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid2)).Should().NotThrow().Which;
        handle.Rid.Should().Be(rid2);
        var resource2 = handle.Value.Should().BeOfType<ReferenceResource>().Which;
        resource2.Value.Should().Be(420);

        resource1.Reference.Should().BeSameAs(resource2);
        resource2.Reference.Should().BeNull();
    }
    
    [Fact]
    public void ImportReferenceResource_CircularReference_ShouldHaveCorrectValueAndReference() {
        ResourceID rid3 = ResourceID.Parse("58f0c3e4c7c24f798129d45f248bfa2c");
        ResourceID rid4 = ResourceID.Parse("235be3d4ddfd42fa983ce7c9d9f58d51");
        
        BuildResources(rid3, rid4);
        _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
        
        _fileSystem.File.Exists(_fileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid3}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        _fileSystem.File.Exists(_fileSystem.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid4}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
        
        var handle = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid3)).Should().NotThrow().Which;
        handle.Rid.Should().Be(rid3);
        var resource3 = handle.Value.Should().BeOfType<ReferenceResource>().Which;
        resource3.Value.Should().Be(69);
        
        handle = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid4)).Should().NotThrow().Which;
        handle.Rid.Should().Be(rid4);
        var resource4 = handle.Value.Should().BeOfType<ReferenceResource>().Which;
        resource4.Value.Should().Be(420);
        
        resource3.Reference.Should().BeSameAs(resource4);
        resource4.Reference.Should().BeSameAs(resource3);
    }
}
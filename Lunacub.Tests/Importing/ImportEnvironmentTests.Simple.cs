namespace Caxivitual.Lunacub.Tests.Importing;

partial class ImportEnvironmentTests {
    [Fact]
    public void SimpleResource_ShouldBeSuccess() {
        ResourceID rid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
        
        var fs = ((MockResourceLibrary)_env.Input.Libraries[0]).FileSystem;
        fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();

        new Func<object?>(() => _env.Import<object>(rid)).Should().NotThrow()
            .Which.Should().BeOfType<SimpleResource>()
            .Which.Value.Should().Be(69);
    }
}
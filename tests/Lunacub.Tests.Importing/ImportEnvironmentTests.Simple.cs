using FluentAssertions.Extensions;

namespace Caxivitual.Lunacub.Tests.Importing;

partial class ImportEnvironmentTests {
    [Fact]
    public async Task ImportSimpleResource_Normal_DeserializeCorrectly() {
        _buildEnvironment.Resources.Add(1, new("Resource", [], new() {
            Provider = MemoryResourceProvider.AsUtf8("""{"Value":69}""", DateTime.MinValue),
            Options = new() {
                ImporterName = nameof(ResourceWithValueImporter),
            },
        }));

        _buildEnvironment.BuildResources();

        _importEnvironment.Libraries.Add(new MockResourceLibrary(_fileSystem));

        var result = (await new Func<Task<ResourceHandle<object>>>(() => _importEnvironment.Import<object>(1).Task)
            .Should()
            .CompleteWithinAsync(0.5.Seconds(), "Simple resource shouldn't take this long."))
            .Subject;

        result.ResourceId.Should().Be((ResourceID)1);
        result.Value.Should().NotBeNull().And.BeOfType<ResourceWithValue>().Which.Value.Should().Be(69);
    }
}
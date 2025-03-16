using Caxivitual.Lunacub.Compilation;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Caxivitual.Lunacub.Tests;

public class MockResourceLibrary(Guid id, MockFileSystem fs) : ResourceLibrary(id) {
    public override bool Contains(ResourceID rid) {
        return fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}"));
    }

    public override Stream CreateStream(ResourceID rid) {
        return new MockFileStream(fs, Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}"), FileMode.Open, FileAccess.Read);
    }
}
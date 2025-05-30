﻿// namespace Caxivitual.Lunacub.Tests.Importing;
//
// partial class ImportEnvironmentTests {
//     [Fact]
//     public void ImportSimpleResource_Normal_DeserializeCorrectly() {
//         ResourceID rid = ResourceID.Parse("e0b8066bf60043c5a0c3a7782363427d");
//
//         BuildResources(rid);
//
//         _importEnv.Input.Libraries.Add(new MockResourceLibrary(Guid.NewGuid(), _fileSystem));
//         
//         var fs = ((MockResourceLibrary)_importEnv.Input.Libraries[0]).FileSystem;
//         fs.File.Exists(fs.Path.Combine(MockOutputSystem.ResourceOutputDirectory, $"{rid}{CompilingConstants.CompiledResourceExtension}")).Should().BeTrue();
//
//         var handle = new Func<ResourceHandle<object>>(() => _importEnv.Import<object>(rid)).Should().NotThrow().Which;
//         handle.Rid.Should().Be(rid);
//         handle.Value.Should().BeOfType<SimpleResource>().Which.Value.Should().Be(69);
//     }
// }
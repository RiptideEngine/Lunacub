// using Caxivitual.Lunacub.Building.Core;
// using Caxivitual.Lunacub.Importing.Core;
// using System.Collections.Immutable;
//
// namespace Caxivitual.Lunacub.Tests.Importing;
//
// public class ImportStatisticsTests {
//     private readonly Dictionary<ResourceID, ImmutableArray<byte>> _compiledResources;
//     private readonly ImportEnvironment _importEnvironment;
//     
//     public ImportStatisticsTests() {
//         Dictionary<ResourceID, (DateTime, ImmutableArray<byte>)> resources = [];
//         
//         using BuildEnvironment buildEnv = new(new MemoryOutputSystem(new Dictionary<ResourceID, IncrementalInfo>(), resources));
//         buildEnv.Importers.Add(nameof(SimpleResourceImporter), new SimpleResourceImporter());
//         buildEnv.SerializerFactories.Add(new SimpleResourceSerializerFactory());
//         buildEnv.Resources.Add(new("e0b8066bf60043c5a0c3a7782363427d"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "SimpleResource.json"), new(nameof(SimpleResourceImporter)));
//
//         buildEnv.BuildResources();
//
//         _compiledResources = resources.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Item2)).ToDictionary();
//         
//         _importEnvironment = new() {
//             Deserializers = {
//                 [nameof(SimpleResourceDeserializer)] = new SimpleResourceDeserializer(),
//             },
//         };
//     }
//     
//     [Theory]
//     [InlineData(1)]
//     [InlineData(100)]
//     [InlineData(10000)]
//     public async Task ImportOnly_ParallelMultipleTimes_HaveCorrectStatistics(uint times) {
//         // IEnumerable<ResourceID> resourceIds = Enumerable.Range(0, (int)times).Select(i => new ResourceID(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)i))).ToArray();
//         //
//         // _importEnvironment.Input.Libraries.Add(new MemoryResourceLibrary(Guid.NewGuid(), resourceIds.ToDictionary(rid => rid, _ => _compiledResources.Values.First())));
//         //
//         // // await Task.WhenAll(resourceIds.Select(rid => _importEnvironment.ImportAsync<SimpleResource>(rid).Task));
//         //
//         // _importEnvironment.Statistics.TotalReferenceCount.Should().Be(times);
//         // _importEnvironment.Statistics.TotalDisposeCount.Should().Be(0);
//         // _importEnvironment.Statistics.UniqueResourceCount.Should().Be(times);
//         // _importEnvironment.Statistics.DisposedResourceCount.Should().Be(0);
//         // _importEnvironment.Statistics.UndisposedResourceCount.Should().Be(0);
//     }
//
//     [Fact]
//     public async Task ImportThenDispose_x1FinishWithDisposer_HaveCorrectStatistics() {
//         // _importEnvironment.Input.Libraries.Add(new MemoryResourceLibrary(Guid.NewGuid(), _compiledResources));
//         // _importEnvironment.Disposers.Add(new DisposableDisposer());
//         //
//         // await _importEnvironment.ImportAsync<SimpleResource>(new("e0b8066bf60043c5a0c3a7782363427d")).Task;
//         // _importEnvironment.Release(new ResourceID("e0b8066bf60043c5a0c3a7782363427d")).Should().Be(ReleaseStatus.Success);
//         //
//         // _importEnvironment.Statistics.TotalReferenceCount.Should().Be(1);
//         // _importEnvironment.Statistics.TotalDisposeCount.Should().Be(0);
//         // _importEnvironment.Statistics.UniqueResourceCount.Should().Be(100);
//         // _importEnvironment.Statistics.DisposedResourceCount.Should().Be(0);
//         // _importEnvironment.Statistics.UndisposedResourceCount.Should().Be(0);
//     }
// }
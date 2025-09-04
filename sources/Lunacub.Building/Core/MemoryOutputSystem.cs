using Caxivitual.Lunacub.Building.Collections;
using Caxivitual.Lunacub.Collections;
using System.Collections.Frozen;
using ResourceOutput = (System.Collections.Immutable.ImmutableArray<byte>, System.DateTime);

namespace Caxivitual.Lunacub.Building.Core;

public sealed class MemoryOutputSystem : OutputSystem {
    public LibraryIdentityDictionary<LibraryOutput> Outputs { get; } = [];

    public EnvironmentIncrementalInfos IncrementalInfos { get; } = [];
    
    public override void CollectIncrementalInfos(EnvironmentIncrementalInfos receiver) {
        foreach ((var libraryId, var libraryIncrementalInfos) in IncrementalInfos) {
            LibraryIncrementalInfos receiverLibraryIncrementalInfos = [];

            foreach ((var resourceId, var incrementalInfos) in libraryIncrementalInfos) {
                receiverLibraryIncrementalInfos.Add(resourceId, incrementalInfos);
            }
            
            receiver.Add(libraryId, receiverLibraryIncrementalInfos);
        }
    }

    public override void FlushIncrementalInfos(EnvironmentIncrementalInfos envIncrementalInfos) {
        IncrementalInfos.Clear();
        
        foreach ((var libraryId, var libraryIncrementalInfos) in envIncrementalInfos) {
            if (IncrementalInfos.TryGetValue(libraryId, out var receiverLibraryIncrementalInfos)) {
                foreach ((var resourceId, var incrementalInfos) in libraryIncrementalInfos) {
                    receiverLibraryIncrementalInfos[resourceId] = incrementalInfos;
                }
            } else {
                receiverLibraryIncrementalInfos = [];

                foreach ((var resourceId, var incrementalInfos) in libraryIncrementalInfos) {
                    receiverLibraryIncrementalInfos.Add(resourceId, incrementalInfos);
                }
            
                IncrementalInfos.Add(libraryId, receiverLibraryIncrementalInfos);
            }
        }
    }

    public override void CopyCompiledResourceOutput(Stream sourceStream, ResourceAddress address) {
        if (!Outputs.TryGetValue(address.LibraryId, out var libraryOutput)) {
            libraryOutput = new([], []);
            Outputs.Add(address.LibraryId, libraryOutput);
        }
        
        byte[] buffer = new byte[sourceStream.Length];
        sourceStream.ReadExactly(buffer, 0, buffer.Length);
        
        libraryOutput.CompiledResources[address.ResourceId] = (ImmutableCollectionsMarshal.AsImmutableArray(buffer), DateTime.Now);
    }

    public override void OutputLibraryRegistry(ResourceRegistry<ResourceRegistry.Element> registry, LibraryID libraryId) {
        if (!Outputs.TryGetValue(libraryId, out var libraryOutput)) return;

        libraryOutput.Registry.Clear();
        
        foreach ((var resourceId, var element) in registry) {
            libraryOutput.Registry.Add(resourceId, element);
        }
    }

    public override DateTime? GetResourceLastBuildTime(ResourceAddress address) {
        if (!Outputs.TryGetValue(address.LibraryId, out var libraryOutput)) return null;

        return libraryOutput.CompiledResources.TryGetValue(address.ResourceId, out ResourceOutput output) ? output.Item2 : null;
    }

    public readonly record struct LibraryOutput(
        Dictionary<ResourceID, ResourceOutput> CompiledResources,
        ResourceRegistry<ResourceRegistry.Element> Registry
    );
}
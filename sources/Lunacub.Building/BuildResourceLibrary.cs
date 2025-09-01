using Caxivitual.Lunacub.Building.Incremental;
using Caxivitual.Lunacub.Exceptions;
using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

public sealed class BuildResourceLibrary : ResourceLibrary {
    private readonly BuildSourceProvider _provider;

    public ResourceRegistry<ResourceRegistry.Element<BuildingResource>> Registry { get; }
    
    public BuildResourceLibrary(LibraryID id, BuildSourceProvider sourceProvider) : base(id) {
        _provider = sourceProvider;
        Registry = [];
    }

    public SourceStreams CreateSourceStreams(ResourceID resourceId) {
        if (!Registry.TryGetValue(resourceId, out ResourceRegistry.Element<BuildingResource> element)) {
            return new(null, FrozenDictionary<string, Stream?>.Empty);
        }

        SourceAddresses addresses = element.Option.Addresses;

        Stream? primaryStream = _provider.CreateStream(addresses.Primary);

        try {
            int secondaryStreamsCount = addresses.Secondaries.Count;
            
            if (secondaryStreamsCount > 0) {
                Dictionary<string, Stream?> secondaryStreams = new(secondaryStreamsCount);

                try {
                    foreach ((var name, var address) in addresses.Secondaries) {
                        secondaryStreams.Add(name, _provider.CreateStream(address));
                    }
                } catch {
                    foreach (var stream in secondaryStreams.Values) {
                        stream?.Dispose();
                    }
                    
                    throw;
                }

                return new(primaryStream, secondaryStreams);
            }
            
            return new(primaryStream, FrozenDictionary<string, Stream?>.Empty);
        } catch {
            primaryStream?.Dispose();
            throw;
        }
    }

    public SourcesInfo GetSourcesInformations(ResourceID resourceId) {
        if (!Registry.TryGetValue(resourceId, out ResourceRegistry.Element<BuildingResource> element)) {
            return new(default, FrozenDictionary<string, SourceInfo>.Empty);
        }
        
        var sourceAddresses = element.Option.Addresses;
        SourceInfo primary = new(sourceAddresses.Primary, _provider.GetLastWriteTime(sourceAddresses.Primary));
        
        if (sourceAddresses.Secondaries.Count > 0) {
            Dictionary<string, SourceInfo> secondaries = [];
            
            foreach ((var name, var address) in sourceAddresses.Secondaries) {
                secondaries.Add(name, new(address, _provider.GetLastWriteTime(address)));
            }
    
            return new(primary, secondaries);
        }
        
        return new(primary);
    }
}
namespace Caxivitual.Lunacub.Building;

partial class BuildSession {
    private void ProcessPostBuild() {
        Log.FlushingIncrementalInformations(_environment.Logger);
        
        // Merge our procedural schematic to environment.
        
        // The override procedural schematic only contains successfully build resource, it doesn't contains cached resources.
        foreach ((var libraryId, var overrideLibraryProceduralSchematic) in _overrideProceduralSchematic) {
            if (_environment.ProceduralSchematic.TryGetValue(libraryId, out var envLibraryProceduralSchematic)) {
                envLibraryProceduralSchematic.Clear();
                
                foreach ((var resourceId, var overrideSchematic) in overrideLibraryProceduralSchematic) {
                    envLibraryProceduralSchematic.Add(resourceId, overrideSchematic);
                }
            } else {
                // Newly built library?
                _environment.ProceduralSchematic.Add(libraryId, overrideLibraryProceduralSchematic);
            }
        }
        
        // When the resource is cached, the registry does not contains our procedural generated resource.
        // This is where the procedural schematic came into used.
        
        foreach ((LibraryID libraryId, ResourceRegistry<ResourceRegistry.Element> registry) in _outputRegistries) {
            // How this processing works:
            // Enumerate through the resource which got cached, convert the old procedural schematic into the registry element
            // and add it to the registry.
        
            // If the library got procedural schematic.
            // if (_environment.ProceduralSchematic.TryGetValue(libraryId, out var libraryProceduralSchematic)) {
            //     bool getSuccessful = Results.TryGetValue(libraryId, out var libraryResults);
            //     Debug.Assert(getSuccessful);
            //
            //     foreach ((var resourceId, var result) in libraryResults!) {
            //         if (result.Status != BuildStatus.Cached) continue;
            //
            //         getSuccessful = libraryProceduralSchematic.TryGetValue(resourceId, out var resourceProceduralSchematic);
            //         Debug.Assert(getSuccessful);
            //
            //         foreach ((var proceduralResourceId, var tags) in resourceProceduralSchematic!) {
            //             registry.Add(proceduralResourceId, new(null, tags));
            //         }
            //     }
            // }
        
            // Output the registry.
            _environment.Output.OutputLibraryRegistry(registry, libraryId);
        }
        
        _environment.FlushProceduralSchematic();
        _environment.FlushIncrementalInfos();
    }
}
﻿using Caxivitual.Lunacub.Compilation;

namespace Caxivitual.Lunacub.Importing.Core;

[ExcludeFromCodeCoverage]
public sealed class FileResourceLibrary(string directory) : ResourceLibrary {
    public string Directory { get; } = directory;
    
    public override bool Contains(ResourceID rid) {
        return File.Exists(Path.Combine(Directory, $"{rid}{CompilingConstants.CompiledResourceExtension}"));
    }

    protected override Stream? CreateStreamImpl(ResourceID rid) {
        string path = Path.Combine(Directory, $"{rid}{CompilingConstants.CompiledResourceExtension}");
        
        return File.Exists(path) ? File.OpenRead(path) : null;
    }

    public override IEnumerator<ResourceID> GetEnumerator() {
        foreach (var file in System.IO.Directory.EnumerateFiles(Directory, $"*{CompilingConstants.CompiledResourceExtension}")) {
            if (ResourceID.TryParse(file.AsSpan(..^CompilingConstants.CompiledResourceExtension.Length), out var rid)) {
                yield return rid;
            }
        }
    }
}
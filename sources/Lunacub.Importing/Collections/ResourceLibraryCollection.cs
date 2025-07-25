﻿using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing.Collections;

public sealed class ResourceLibraryCollection : Collection<ImportResourceLibrary> {
    public bool ContainsResource(ResourceID resourceId) {
        foreach (var library in this) {
            if (library.Registry.ContainsKey(resourceId)) return true;
        }

        return false;
    }
    
    public bool ContainsResource(ResourceID resourceId, out ResourceRegistry.Element output) {
        foreach (var library in this) {
            if (library.Registry.TryGetValue(resourceId, out output)) {
                return true;
            }
        }

        output = default;
        return false;
    }
    
    public bool ContainsResource(ReadOnlySpan<char> name) {
        foreach (var library in this) {
            if (library.Registry.ContainsName(name)) return true;
        }

        return false;
    }
    
    public bool ContainsResource(ReadOnlySpan<char> name, out ResourceID output) {
        foreach (var library in this) {
            if (library.Registry.TryGetValue(name, out output)) {
                return true;
            }
        }

        output = default;
        return false;
    }
    
    public bool ContainsResource(ReadOnlySpan<char> name, out ResourceRegistry.Element output) {
        foreach (var library in this) {
            if (library.Registry.TryGetValue(name, out output)) {
                return true;
            }
        }

        output = default;
        return false;
    }
    
    public Stream? CreateResourceStream(ResourceID resourceId) {
        foreach (var library in this) {
            if (library.CreateResourceStream(resourceId) is { } stream) return stream;
        }

        return null;
    }
    
    protected override void InsertItem(int index, ImportResourceLibrary item) {
        ValidateLibrary(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, ImportResourceLibrary item) {
        ValidateLibrary(item);
        
        base.SetItem(index, item);
    }

    private void ValidateLibrary(ImportResourceLibrary item) {
        ArgumentNullException.ThrowIfNull(item);
        
        // TODO: Validate ID, name collision.
    }
}
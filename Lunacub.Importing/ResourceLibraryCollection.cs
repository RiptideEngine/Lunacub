﻿using System.Collections.ObjectModel;

namespace Caxivitual.Lunacub.Importing;

public sealed class ResourceLibraryCollection : Collection<ResourceLibrary> {
    public bool Remove(Guid id) {
        for (int i = 0; i < Count; i++) {
            if (this[i].Id == id) {
                RemoveAt(i);
                return true;
            }
        }
        
        return false;
    }
    
    protected override void InsertItem(int index, ResourceLibrary item) {
        ArgumentNullException.ThrowIfNull(item, nameof(item));
        ValidateLibrary(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, ResourceLibrary item) {
        ArgumentNullException.ThrowIfNull(item, nameof(item));

        if (!ReferenceEquals(item, this[index])) {
            ValidateLibrary(item);

            base.SetItem(index, item);
        }
    }
    
    [StackTraceHidden]
    private void ValidateLibrary(ResourceLibrary item) {
        foreach (var library in this) {
            if (library.Id == item.Id) {
                throw new ArgumentException($"An instance of {nameof(ResourceLibrary)} with ID {item.Id} already present.", nameof(item));
            }
        }
    }
}
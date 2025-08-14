using System.Collections.Immutable;

namespace Caxivitual.Lunacub;

[CollectionBuilder(typeof(TagCollection), nameof(Create))]
partial struct TagCollection {
    public static TagCollection Create(string tag) {
        ValidateTag(tag);
        return new([tag]);
    }
    
    public static TagCollection Create(string tag1, string tag2) {
        ValidateTag(tag1);
        ValidateTag(tag2);
        
        return new([tag1, tag2]);
    }

    public static TagCollection Create(string tag1, string tag2, string tag3) {
        ValidateTag(tag1);
        ValidateTag(tag2);
        ValidateTag(tag3);
        
        return new([tag1, tag2, tag3]);
    }
    
    public static TagCollection Create(string tag1, string tag2, string tag3, string tag4) {
        ValidateTag(tag1);
        ValidateTag(tag2);
        ValidateTag(tag3);
        ValidateTag(tag4);
        
        return new([tag1, tag2, tag3, tag4]);
    }

    public static TagCollection Create(ImmutableArray<string> tags) {
        foreach (var tag in tags) {
            ValidateTag(tag);
        }

        return new(tags);
    }
    
    public static TagCollection Create(ImmutableArray<string> tags, int start, int length) {
        for (int i = start; i < start + length; i++){
            ValidateTag(tags[i]);
        }
        
        return new(ImmutableArray.Create(tags, start, length));
    }
    
    public static TagCollection Create(params string[] tags) {
        foreach (var tag in tags) {
            ValidateTag(tag);
        }
        
        return new([..tags]);
    }
    
    public static TagCollection Create(string[] tags, int start, int length) {
        for (int i = start; i < start + length; i++){
            ValidateTag(tags[i]);
        }
        
        return new(ImmutableArray.Create(tags, start, length));
    }

    public static TagCollection Create(params ReadOnlySpan<string> tags) {
        foreach (var tag in tags) {
            ValidateTag(tag);
        }

        return new([..tags]);
    }
}
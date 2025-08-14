using Caxivitual.Lunacub.Serialization;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Caxivitual.Lunacub;

[JsonConverter(typeof(TagCollectionConverter))]
public readonly partial struct TagCollection : IReadOnlyList<string> {
    public static ReadOnlySpan<char> ValidCharacters => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
    
    private readonly ImmutableArray<string> _tags;
    
    public int Count => _tags.Length;

    private TagCollection(ImmutableArray<string> tags) {
        _tags = tags;
    }
    
    public string this[int index] => _tags[index];
    
    public ImmutableArray<string>.Enumerator GetEnumerator() => _tags.GetEnumerator();
    
    IEnumerator<string> IEnumerable<string>.GetEnumerator() => ((IEnumerable<string>)_tags).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_tags).GetEnumerator();

    public static bool IsValidTag([NotNullWhen(true)] string? tag) {
        if (string.IsNullOrWhiteSpace(tag)) return false;
        
        if (tag.AsSpan().IndexOfAnyExcept(ValidCharacters) is var index && index != -1) {
            return false;
        }

        return true;
    }
    
    private static void ValidateTag(string tag) {
        if (string.IsNullOrWhiteSpace(tag)) {
            throw new ArgumentException(ExceptionMessages.DisallowNullEmptyOrWhiteSpaceTag);
        }
            
        if (tag.AsSpan().IndexOfAnyExcept(ValidCharacters) is var index && index != -1) {
            string message = string.Format(ExceptionMessages.InvalidTagCharacter, tag, index);
            throw new ArgumentException(message);
        }
    }
}
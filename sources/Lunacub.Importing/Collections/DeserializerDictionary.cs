using Caxivitual.Lunacub.Collections;

namespace Caxivitual.Lunacub.Importing.Collections;

[ExcludeFromCodeCoverage]
public sealed class DeserializerDictionary : IdentityDictionary<Deserializer> {
    internal DeserializerDictionary() : base(StringComparer.Ordinal) { }
    
    protected override void Validate(ReadOnlySpan<char> key, Deserializer value) {
        base.Validate(key, value);
        
        if (key.IsWhiteSpace()) {
            throw new ArgumentException(ExceptionMessages.EmptyOrWhitespaceKey, nameof(key));
        }
    }

    protected override bool TryValidate(ReadOnlySpan<char> key, Deserializer value) {
        return base.TryValidate(key, value) && !key.IsWhiteSpace();
    }
}
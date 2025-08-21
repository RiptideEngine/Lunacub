using Caxivitual.Lunacub.Collections;

namespace Caxivitual.Lunacub.Building.Collections;

/// <summary>
/// Represents a dictionary of <see cref="Importer"/> that uses <see cref="string"/> as key.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ImporterDictionary : IdentityDictionary<Importer> {
    internal ImporterDictionary() : base(StringComparer.Ordinal) { }
    
    protected override void Validate(ReadOnlySpan<char> key, Importer value) {
        base.Validate(key, value);
        
        if (key.IsWhiteSpace()) {
            throw new ArgumentException(ExceptionMessages.EmptyOrWhitespaceKey, nameof(key));
        }
    }

    protected override bool TryValidate(ReadOnlySpan<char> key, Importer value) {
        return base.TryValidate(key, value) && !key.IsWhiteSpace();
    }
}
using Caxivitual.Lunacub.Collections;

namespace Caxivitual.Lunacub.Building.Collections;

/// <summary>
/// Represents a dictionary of <see cref="Processor"/> that uses <see cref="string"/> as key.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ProcessorDictionary : IdentityDictionary<Processor> {
    internal ProcessorDictionary() : base(StringComparer.Ordinal) { }

    protected override void Validate(ReadOnlySpan<char> key, Processor value) {
        base.Validate(key, value);
        
        if (key.IsWhiteSpace()) {
            throw new ArgumentException(ExceptionMessages.EmptyOrWhitespaceKey, nameof(key));
        }
    }

    protected override bool TryValidate(ReadOnlySpan<char> key, Processor value) {
        return base.TryValidate(key, value) && !key.IsWhiteSpace();
    }
}
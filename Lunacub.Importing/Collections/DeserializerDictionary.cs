namespace Caxivitual.Lunacub.Importing.Collections;

[ExcludeFromCodeCoverage]
public sealed class DeserializerDictionary : IdentityDictionary<Deserializer> {
    internal DeserializerDictionary() : base(StringComparer.Ordinal) { }
    
    public override void Add(string key, Deserializer value) {
        ValidateKey(key);
        ValidateValue(value);
        
        _dict.Add(key, value);
    }

    public override bool TryAdd(string key, Deserializer value) {
        ValidateKey(key);
        ValidateValue(value);
        
        return _dict.TryAdd(key, value);
    }

    public override bool TryAdd(ReadOnlySpan<char> key, Deserializer value) {
        ValidateKey(key);
        ValidateValue(value);

        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().TryAdd(key, value);
    }

    public override bool Remove(string key) {
        return _dict.Remove(key);
    }

    public override bool Remove(string key, [NotNullWhen(true)] out Deserializer? output) {
        return _dict.Remove(key, out output);
    }

    public override bool Remove(ReadOnlySpan<char> key) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().Remove(key);
    }

    public override bool Remove(ReadOnlySpan<char> key, [NotNullWhen(true)] out Deserializer? output) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().Remove(key, out _, out output);
    }

    public override bool ContainsKey(string key) {
        return _dict.ContainsKey(key);
    }

    public override bool ContainsKey(ReadOnlySpan<char> key) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().ContainsKey(key);
    }

    public override bool TryGetValue(string key, [MaybeNullWhen(false)] out Deserializer output) {
        return _dict.TryGetValue(key, out output);
    }
    
    public override bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out Deserializer output) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(key, out output);
    }
    
    public override Deserializer this[string key] {
        get => _dict[key];
        set {
            ValidateKey(key);
            ValidateValue(value);
            
            _dict[key] = value;
        }
    }

    public override Deserializer this[ReadOnlySpan<char> key] {
        get => _dict.GetAlternateLookup<ReadOnlySpan<char>>()[key];
        set {
            ValidateKey(key);
            ValidateValue(value);
            
            var lookup = _dict.GetAlternateLookup<ReadOnlySpan<char>>();
            lookup[key] = value;
        }
    }
    
    private static void ValidateKey(string key, [CallerArgumentExpression(nameof(key))] string? expression = "") {
        ArgumentException.ThrowIfNullOrWhiteSpace(key, expression);
    }

    private static void ValidateKey(ReadOnlySpan<char> key, [CallerArgumentExpression(nameof(key))] string? expression = "") {
        if (key.IsEmpty || key.IsWhiteSpace()) {
            throw new ArgumentException(ExceptionMessages.EmptyOrWhitespaceKey, expression);
        }
    }
    
    private static void ValidateValue(Deserializer value, [CallerArgumentExpression(nameof(value))] string? expression = "") {
        ArgumentNullException.ThrowIfNull(value, expression);
    }
}
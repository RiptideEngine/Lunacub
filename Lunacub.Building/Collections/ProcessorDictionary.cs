namespace Caxivitual.Lunacub.Building.Collections;

[ExcludeFromCodeCoverage]
public sealed class ProcessorDictionary : IdentityDictionary<Processor> {
    internal ProcessorDictionary() : base(StringComparer.Ordinal) { }
    
    public override void Add(string key, Processor value) {
        ValidateKey(key);
        ValidateValue(value);
        
        _dict.Add(key, value);
    }

    public override bool TryAdd(string key, Processor value) {
        ValidateKey(key);
        ValidateValue(value);
        
        return _dict.TryAdd(key, value);
    }

    public override bool TryAdd(ReadOnlySpan<char> key, Processor value) {
        ValidateKey(key);
        ValidateValue(value);

        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().TryAdd(key, value);
    }

    public override bool Remove(string key) {
        return _dict.Remove(key);
    }

    public override bool Remove(string key, [NotNullWhen(true)] out Processor? value) {
        return _dict.Remove(key, out value);
    }

    public override bool Remove(ReadOnlySpan<char> key) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().Remove(key);
    }

    public override bool Remove(ReadOnlySpan<char> key, [NotNullWhen(true)] out Processor? value) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().Remove(key, out _, out value);
    }

    public override bool ContainsKey(string key) {
        return _dict.ContainsKey(key);
    }

    public override bool ContainsKey(ReadOnlySpan<char> key) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().ContainsKey(key);
    }

    public override bool TryGetValue(string key, [MaybeNullWhen(false)] out Processor value) {
        return _dict.TryGetValue(key, out value);
    }
    
    public override bool TryGetValue(ReadOnlySpan<char> key, [MaybeNullWhen(false)] out Processor value) {
        return _dict.GetAlternateLookup<ReadOnlySpan<char>>().TryGetValue(key, out value);
    }
    
    public override Processor this[string key] {
        get => _dict[key];
        set {
            ValidateKey(key);
            ValidateValue(value);
            
            _dict[key] = value;
        }
    }

    public override Processor this[ReadOnlySpan<char> key] {
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
    
    private static void ValidateValue(Processor value, [CallerArgumentExpression(nameof(value))] string? expression = "") {
        ArgumentNullException.ThrowIfNull(value, expression);
    }
}
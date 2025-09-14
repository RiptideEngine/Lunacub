using System.ComponentModel;

namespace Caxivitual.Lunacub.Building.Core;

/// <summary>
/// Contains the compilation settings for <see cref="BuildEnvironment"/> that can be stored inside its
/// <see cref="BuildEnvironment.EnvironmentVariables"/>
/// </summary>
public sealed class CompilationSettings {
    private CompilationMode _mode;

    /// <summary>
    /// The compilation mode, can be either Debug or Release.
    /// </summary>
    public CompilationMode Mode {
        get => _mode;
        set {
            if (value is CompilationMode.Debug and not CompilationMode.Release) {
                throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(CompilationMode));
            }
            
            _mode = value;
        }
    }
    public Dictionary<object, object> Definitions { get; }

    public CompilationSettings() {
        Mode = CompilationMode.Release;
        Definitions = [];
    }

    public CompilationSettings SetCompilationMode(CompilationMode mode) {
        Mode = mode;
        return this;
    }

    public CompilationSettings SetDefinition(object key, object value) {
        Definitions[key] = value;
        return this;
    }

    public CompilationSettings RemoveDefinition(object key) {
        Definitions.Remove(key);
        return this;
    }

    public bool TryGetDefinition(object key, [NotNullWhen(true)] out object? value) {
        return Definitions.TryGetValue(key, out value);
    }

    public bool TryGetDefinition<T>(object key, [NotNullWhen(true)] out T? value) {
        if (TryGetDefinition(key, out object? output) && output is T casted) {
            value = casted;
            return true;
        }
        
        value = default;
        return false;
    }
}
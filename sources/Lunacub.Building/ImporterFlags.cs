namespace Caxivitual.Lunacub.Building;

[Flags]
public enum ImporterFlags {
    /// <summary>
    /// Indicates the <see cref="Importer"/> has no special flag.
    /// </summary>
    Default = 0,
    
    /// <summary>
    /// Indicates the <see cref="Importer"/> do not handle dependency importing. Allows for optimization by skipping dependency
    /// extraction during build graph construction.
    /// </summary>
    NoDependency = 1,
}
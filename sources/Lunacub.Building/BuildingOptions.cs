using Caxivitual.Lunacub.Building.Serialization;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// A structure contains the informations needed for <see cref="BuildEnvironment"/> to build a resource.
/// </summary>
[JsonConverter(typeof(BuildingOptionsConverter))]
public readonly struct BuildingOptions : IEquatable<BuildingOptions> {
    /// <summary>
    /// Gets the name of <see cref="Importer"/> used to import resource data into a <see cref="ContentRepresentation"/>.
    /// </summary>
    public required string ImporterName { get; init; }
    
    /// <summary>
    /// Gets the name of <see cref="Processor"/> used to convert the <see cref="ContentRepresentation"/> after the
    /// importing stage into another <see cref="ContentRepresentation"/>.
    /// </summary>
    public string? ProcessorName { get; init; }
    
    /// <summary>
    /// Gets the importing option object that can be used for building resource.
    /// </summary>
    public IImportOptions? Options { get; init; }

    /// <summary>
    /// Initializes the structure with empty <see cref="ProcessorName"/> and empty <see cref="Tags"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public BuildingOptions() {
        ProcessorName = string.Empty;
    }
    
    /// <summary>
    /// Initializes the structure with provided <paramref name="importerName"/> and <see cref="processorName"/> and <see langword="null"/> <see cref="Options"/>.
    /// </summary>
    /// <param name="importerName">
    ///     The importer name in <see cref="BuildEnvironment.Importers"/> used to build resource.
    /// </param>
    /// <param name="processorName">
    ///     The processor name in <see cref="BuildEnvironment.Processors"/> used to process resource from a
    ///     <see cref="ContentRepresentation"/> to another.
    /// </param>
    [SetsRequiredMembers]
    public BuildingOptions(string importerName, string? processorName = null) : this(importerName, processorName, null) { }
    
    /// <summary>
    /// Initializes the structure with provided <paramref name="importerName"/> and <see cref="processorName"/> <see langword="null"/> <see cref="Options"/>.
    /// </summary>
    /// <param name="importerName">
    ///     The importer name in <see cref="BuildEnvironment.Importers"/> used to build resource.
    /// </param>
    /// <param name="processorName">
    ///     The processor name in <see cref="BuildEnvironment.Processors"/> used to process resource from a
    ///     <see cref="ContentRepresentation"/> to another.
    /// </param>
    /// <param name="options">
    ///     The importing option object that can be used to finely tune the resource building process.
    /// </param>
    [SetsRequiredMembers]
    public BuildingOptions(string importerName, string? processorName, IImportOptions? options) {
        ImporterName = importerName;
        ProcessorName = processorName;
        Options = options;
    }

    public bool Equals(BuildingOptions other) {
        return ImporterName == other.ImporterName && ProcessorName == other.ProcessorName &&
               (Options?.Equals(other.Options) ?? other.Options == null);
    }

    [ExcludeFromCodeCoverage]
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is BuildingOptions other && Equals(other);
    
    [ExcludeFromCodeCoverage]
    public override int GetHashCode() {
        HashCode hc = new();
        
        hc.Add(ImporterName);
        hc.Add(ProcessorName);
        hc.Add(Options);
        
        return hc.ToHashCode();
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.Append(nameof(BuildingOptions));
        sb.Append(" { ");
        sb.Append(nameof(ImporterName)).Append('=').Append(ImporterName).Append(", ");
        sb.Append(nameof(ProcessorName)).Append('=').Append(ProcessorName).Append(", ");
        sb.Append(nameof(Options)).Append(" = ").Append(Options);
        sb.Append(" }");
        
        return sb.ToString();
    }

    [ExcludeFromCodeCoverage] public static bool operator ==(BuildingOptions left, BuildingOptions right) => left.Equals(right);
    [ExcludeFromCodeCoverage] public static bool operator !=(BuildingOptions left, BuildingOptions right) => !(left == right);
}
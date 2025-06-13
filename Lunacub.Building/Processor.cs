namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the process of processing a <see cref="ContentRepresentation"/> and convert it
/// into a different <see cref="ContentRepresentation"/> type before serialization.
/// </summary>
public abstract class Processor {
    public virtual string? Version => null;
    
    internal static Processor Passthrough { get; } = new PassthroughProcessor();
    
    internal abstract bool CanProcess(ContentRepresentation input);

    // public abstract IReadOnlyDictionary<ProceduralResourceID, BuildingOptions> BuildProceduralResourceSchema();
    
    internal abstract ContentRepresentation Process(ContentRepresentation input, ProcessingContext context);
}

/// <inheritdoc cref="Processor"/>
/// <typeparam name="TInput">The type of object that the processor will operate on, must derived from <see cref="ContentRepresentation"/>.</typeparam>
/// <typeparam name="TOutput">The type of object that the processor will output, must derived from <see cref="ContentRepresentation"/>.</typeparam>
public abstract class Processor<TInput, TOutput> : Processor where TInput : ContentRepresentation where TOutput : ContentRepresentation {
    internal override sealed bool CanProcess(ContentRepresentation input) => input is TInput t && CanProcess(t);
    
    internal override sealed ContentRepresentation Process(ContentRepresentation input, ProcessingContext context) {
        Debug.Assert(input.GetType().IsAssignableTo(typeof(TInput)));

        return Process((TInput)input, context);
    }

    protected virtual bool CanProcess(TInput content) => true;
    protected abstract TOutput Process(TInput input, ProcessingContext context);
}

file sealed class PassthroughProcessor : Processor {
    internal override bool CanProcess(ContentRepresentation input) => true;
    internal override ContentRepresentation Process(ContentRepresentation input, ProcessingContext context) => input;
}
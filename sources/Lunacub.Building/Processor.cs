using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the process of processing a <see cref="ContentRepresentation"/> and convert it
/// into a different <see cref="ContentRepresentation"/> type before serialization.
/// </summary>
public abstract class Processor {
    [ExcludeFromCodeCoverage] public virtual string? Version => null;
    
    internal abstract bool CanProcess(ContentRepresentation input);

    internal abstract ContentRepresentation Process(ContentRepresentation importedObject, ProcessingContext context);
}

/// <inheritdoc cref="Processor"/>
/// <typeparam name="TInput">
///     The type of object that the processor will operate on, must derived from <see cref="ContentRepresentation"/>.
/// </typeparam>
/// <typeparam name="TOutput">
///     The type of object that the processor will output, must derived from <see cref="ContentRepresentation"/>.
/// </typeparam>
public abstract class Processor<TInput, TOutput> : Processor where TInput : ContentRepresentation where TOutput : ContentRepresentation {
    internal override sealed bool CanProcess(ContentRepresentation input) => input is TInput t && CanProcess(t);
    
    internal override sealed ContentRepresentation Process(ContentRepresentation importedObject, ProcessingContext context) {
        Debug.Assert(importedObject.GetType().IsAssignableTo(typeof(TInput)));

        return Process((TInput)importedObject, context);
    }

    protected virtual bool CanProcess(TInput content) => true;
    protected abstract TOutput Process(TInput importedObject, ProcessingContext context);
}
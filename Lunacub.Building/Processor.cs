namespace Caxivitual.Lunacub.Building;

/// <summary>
/// Provides the base class that handles the process of converting a resource into a different type.
/// </summary>
public abstract class Processor {
    internal static Processor Passthrough { get; } = new PassthroughProcessor();
    
    internal abstract bool CanProcess(ContentRepresentation input);
    
    internal abstract ContentRepresentation Process(ContentRepresentation input, ProcessingContext context);
}

/// <inheritdoc cref="Processor"/>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
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
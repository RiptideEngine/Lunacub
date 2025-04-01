namespace Caxivitual.Lunacub.Building;

public abstract class Processor {
    internal static Processor Passthrough { get; } = new PassthroughProcessor();
    
    // Should we use this method, or check whether input can be processed using reflection (getting generic argument)?
    internal abstract bool CanProcess(ContentRepresentation input);
    
    internal abstract ContentRepresentation Process(ContentRepresentation input, ProcessingContext context);
}

public abstract class Processor<TInput, TOutput> : Processor where TInput : ContentRepresentation where TOutput : ContentRepresentation {
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
namespace Caxivitual.Lunacub.Building;

public abstract class Processor {
    internal static Processor Passthrough { get; } = new PassthroughProcessor();
    
    // Should we use this method, or check whether input can be processed using reflection (getting generic argument)?
    internal abstract bool CanProcess(ContentRepresentation input);
    
    internal abstract ContentRepresentation Process(ContentRepresentation input);
}

public abstract class Processor<TInput, TOutput> : Processor where TInput : ContentRepresentation where TOutput : ContentRepresentation {
    internal override sealed ContentRepresentation Process(ContentRepresentation input) {
        Debug.Assert(input.GetType().IsAssignableTo(typeof(TInput)));

        return Process((TInput)input);
    }

    protected virtual bool CanProcess(TInput content) => true;
    protected abstract TOutput Process(TInput input);
    protected virtual void Dispose(TOutput processed) { }
}

file sealed class PassthroughProcessor : Processor {
    internal override bool CanProcess(ContentRepresentation input) => true;
    internal override ContentRepresentation Process(ContentRepresentation input) => input;
}
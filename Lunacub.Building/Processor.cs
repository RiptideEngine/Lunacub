namespace Caxivitual.Lunacub.Building;

public abstract class Processor {
    // Should we use this method, or check whether input can be processed using reflection (getting generic argument)?
    internal abstract bool CanProcess(ContentRepresentation input);
    
    internal abstract ContentRepresentation Process(ContentRepresentation input);
    internal virtual void DisposeObject(ContentRepresentation processed) { }
}

public abstract class Processor<TInput, TOutput> : Processor where TInput : ContentRepresentation where TOutput : ContentRepresentation {
    internal override sealed ContentRepresentation Process(ContentRepresentation input) {
        Debug.Assert(input.GetType().IsAssignableTo(typeof(TInput)));

        return Process((TInput)input);
    }

    internal override sealed void DisposeObject(ContentRepresentation processed) {
        Debug.Assert(processed.GetType().IsAssignableTo(typeof(TOutput)));
        
        Dispose((TOutput)processed);
    }

    protected virtual bool CanProcess(TInput content) => true;
    protected abstract TOutput Process(TInput input);
    protected virtual void Dispose(TOutput processed) { }
}
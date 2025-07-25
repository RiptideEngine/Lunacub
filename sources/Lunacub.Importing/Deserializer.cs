﻿namespace Caxivitual.Lunacub.Importing;

public abstract class Deserializer {
    public abstract Type OutputType { get; }
    
    public abstract Task<object> DeserializeObjectAsync(
        Stream dataStream,
        Stream optionStream,
        DeserializationContext context,
        CancellationToken cancellationToken
    );
    
    public virtual void ResolveReferences(object deserializedObject, DeserializationContext context) { }
}

public abstract class Deserializer<T> : Deserializer where T : class {
    public sealed override Type OutputType => typeof(T);

    public sealed override async Task<object> DeserializeObjectAsync(
        Stream dataStream,
        Stream optionStream,
        DeserializationContext context,
        CancellationToken cancellationToken
    ) {
        return await DeserializeAsync(dataStream, optionStream, context, cancellationToken);
    }

    public sealed override void ResolveReferences(object deserializedObject, DeserializationContext context) {
        ResolveReferences((T)deserializedObject, context);
    }

    protected abstract Task<T> DeserializeAsync(
        Stream dataStream,
        Stream optionStream,
        DeserializationContext context,
        CancellationToken cancellationToken
    );
    
    protected virtual void ResolveReferences(T instance, DeserializationContext context) { }
}
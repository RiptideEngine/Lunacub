namespace Caxivitual.Lunacub.Importing;

public abstract class Deserializer {
    public abstract Type OutputType { get; }
    
    public virtual bool Streaming => false;

    public abstract object DeserializeObject(Stream dataStream, Stream optionStream, DeserializationContext context);
    public virtual void ResolveDependencies(object instance, DeserializationContext context) { }
}

public abstract class Deserializer<T> : Deserializer where T : class {
    public sealed override Type OutputType => typeof(T);

    public sealed override object DeserializeObject(Stream dataStream, Stream optionStream, DeserializationContext context) {
        return Deserialize(dataStream, optionStream, context);
    }

    public sealed override void ResolveDependencies(object instance, DeserializationContext context) {
        ResolveDependencies((T)instance, context);
    }

    protected abstract T Deserialize(Stream dataStream, Stream optionStream, DeserializationContext context);
    protected virtual void ResolveDependencies(T instance, DeserializationContext context) { }
}
namespace Caxivitual.Lunacub.Importing;

public abstract class Deserializer {
    public abstract Type OutputType { get; }

    public abstract object DeserializeObject(Stream stream, DeserializationContext context);
    public virtual void ResolveDependencies(object instance, DeserializationContext context) { }
}

public abstract class Deserializer<T> : Deserializer where T : class {
    public sealed override Type OutputType => typeof(T);

    public sealed override object DeserializeObject(Stream stream, DeserializationContext context) {
        return Deserialize(stream, context)!;
    }

    public sealed override void ResolveDependencies(object instance, DeserializationContext context) {
        ResolveDependencies((T)instance, context);
    }

    protected abstract T Deserialize(Stream stream, DeserializationContext context);
    protected virtual void ResolveDependencies(T instance, DeserializationContext context) { }
}
namespace Caxivitual.Lunacub.Building;

public abstract class Serializer {
    public abstract string DeserializerName { get; }
    
    public abstract bool CanSerialize(Type type);

    internal abstract void SerializeObject(object input, Stream stream);
}

public abstract class Serializer<T> : Serializer {
    public override sealed bool CanSerialize(Type type) => typeof(T).IsAssignableFrom(type);

    internal override sealed void SerializeObject(object input, Stream stream) {
        Serialize((T)input, stream);
    }

    protected abstract void Serialize(T input, Stream stream);
}
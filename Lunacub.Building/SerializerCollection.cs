namespace Caxivitual.Lunacub.Building;

public sealed class SerializerCollection : Collection<Serializer> {
    internal SerializerCollection() {}

    protected override void InsertItem(int index, Serializer item) {
        ArgumentNullException.ThrowIfNull(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, Serializer item) {
        ArgumentNullException.ThrowIfNull(item);
        
        base.SetItem(index, item);
    }
    
    public Serializer? GetSerializable(Type type) {
        ArgumentNullException.ThrowIfNull(type);

        foreach (var serializer in this) {
            if (serializer.CanSerialize(type)) return serializer;
        }

        return null;
    }
}
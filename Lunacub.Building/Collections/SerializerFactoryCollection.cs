namespace Caxivitual.Lunacub.Building.Collections;

[ExcludeFromCodeCoverage]
public sealed class SerializerFactoryCollection : Collection<SerializerFactory> {
    internal SerializerFactoryCollection() {}

    protected override void InsertItem(int index, SerializerFactory item) {
        ArgumentNullException.ThrowIfNull(item);
        
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, SerializerFactory item) {
        ArgumentNullException.ThrowIfNull(item);
        
        base.SetItem(index, item);
    }
    
    public SerializerFactory? GetSerializableFactory(Type type) {
        ArgumentNullException.ThrowIfNull(type);

        foreach (var factory in this) {
            if (factory.CanSerialize(type)) return factory;
        }

        return null;
    }
}
// namespace Caxivitual.Lunacub.Tests.Common;
//
// public sealed class TreeReferencingResource : IDisposable {
//     public TreeReferencingResource[] References { get; set; }
//     public int Value { get; set; }
//     public bool Disposed { get; private set; }
//
//     public void Dispose() {
//         Disposed = true;
//     }
// }
//
// public sealed class TreeReferencingResourceDTO {
//     public ResourceID[] References { get; set; }
//     public int Value { get; set; }
// }
//
// public sealed class TreeReferencingResourceImporter : Importer<TreeReferencingResourceDTO> {
//     protected override TreeReferencingResourceDTO Import(SourceStreams sourceStreams, ImportingContext context) {
//         return JsonSerializer.Deserialize<TreeReferencingResourceDTO>(sourceStreams.PrimaryStream!)!;
//     }
// }
//
// public sealed class TreeReferencingResourceSerializerFactory : SerializerFactory {
//     public override bool CanSerialize(Type representationType) => representationType == typeof(TreeReferencingResourceDTO);
//
//     protected override Serializer CreateSerializer(object serializingObject, SerializationContext context) {
//         return new SerializerCore(serializingObject, context);
//     }
//
//     private sealed class SerializerCore : Serializer {
//         public override string DeserializerName => nameof(TreeReferencingResourceDeserializer);
//
//         public SerializerCore(object obj, SerializationContext context) : base(obj, context) {
//         }
//
//         public override void SerializeObject(Stream outputStream) {
//             TreeReferencingResourceDTO serializing = (TreeReferencingResourceDTO)SerializingObject;
//             
//             using var writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
//         
//             // writer.Write(serializing.Reference);
//             writer.Write(serializing.Value);
//         }
//     }
// }
//
// public sealed class TreeReferencingResourceDeserializer : Deserializer<TreeReferencingResource> {
//     protected override Task<TreeReferencingResource> DeserializeAsync(Stream dataStream, Stream optionsStream, DeserializationContext context, CancellationToken cancellationToken) {
//         using var reader = new BinaryReader(dataStream, Encoding.UTF8, true);
//         
//         int referenceCount = reader.ReadInt32();
//
//         for (int i = 0; i < referenceCount; i++) {
//             context.RequestReference<TreeReferencingResource>((uint)i, reader.ReadResourceID());
//         }
//
//         context.ValueContainer["ReferenceCount"] = referenceCount;
//         
//         return Task.FromResult(new TreeReferencingResource { Value = reader.ReadInt32() });
//     }
//
//     protected override void ResolveReferences(TreeReferencingResource instance, DeserializationContext context) {
//         List<TreeReferencingResource> references = [];
//         
//         for (int i = 0, c = (int)context.ValueContainer["ReferenceCount"]; i < c; i++) {
//             if (context.GetReference<TreeReferencingResource>((uint)i) is { } reference) {
//                 references.Add(reference);
//             }
//         }
//
//         instance.References = references.ToArray();
//     }
// }
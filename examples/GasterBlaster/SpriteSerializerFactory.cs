using Caxivitual.Lunacub.Building;
using Caxivitual.Lunacub.Extensions;
using SixLabors.ImageSharp;
using System.Text;

namespace Caxivitual.Lunacub.Examples.GasterBlaster;

public sealed class SpriteSerializerFactory : SerializerFactory {
    public override bool CanSerialize(Type representationType) => representationType == typeof(SpriteDTO);

    protected override Serializer CreateSerializer(ContentRepresentation serializingObject, SerializationContext context) {
        return new SerializerCore(serializingObject, context);
    }

    private sealed class SerializerCore : Serializer {
        public override string DeserializerName => nameof(SpriteDeserializer);
        
        public SerializerCore(ContentRepresentation serializingObject, SerializationContext context) : base(serializingObject, context) {
        }

        public override void SerializeObject(Stream outputStream) {
            SpriteDTO dto = (SpriteDTO)SerializingObject;

            using BinaryWriter writer = new BinaryWriter(outputStream, Encoding.UTF8, leaveOpen: true);
            writer.Write(dto.Name);
            
            writer.Write(dto.Subsprites.Count);

            foreach (var subsprite in dto.Subsprites) {
                writer.Write(subsprite.Name);
                writer.Write(subsprite.Region.Origin.X);
                writer.Write(subsprite.Region.Origin.Y);
                writer.Write(subsprite.Region.Size.X);
                writer.Write(subsprite.Region.Size.Y);
            }
            
            writer.Write(dto.TextureId);
        }
    }
}
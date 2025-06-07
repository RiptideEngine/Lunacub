namespace Caxivitual.Lunacub.Examples.DependencyImporting;

public sealed class MergingResourceDeserializer : Deserializer<MergingResource> {
    protected override Task<MergingResource> DeserializeAsync(Stream dataStream, Stream optionStream, DeserializationContext context, CancellationToken cancellationToken) {
        using BinaryReader br = new BinaryReader(dataStream);

        int[] array = new int[br.Read7BitEncodedInt()];

        for (int i = 0; i < array.Length; i++) {
            array[i] = br.ReadInt32();
        }

        return Task.FromResult(new MergingResource(array));
    }
}
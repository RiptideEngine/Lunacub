using System.Collections.Frozen;

namespace Caxivitual.Lunacub.Building;

public readonly struct SourceStreams : IDisposable {
    public readonly Stream? PrimaryStream;
    public readonly IReadOnlyDictionary<string, Stream?> SecondaryStreams;

    public SourceStreams(Stream? primaryStream) {
        PrimaryStream = primaryStream;
        SecondaryStreams = FrozenDictionary<string, Stream?>.Empty;
    }

    public SourceStreams(Stream? primaryStream, IReadOnlyDictionary<string, Stream?> secondaryStreams) {
        PrimaryStream = primaryStream;
        SecondaryStreams = secondaryStreams;
    }

    public void Dispose() {
        PrimaryStream?.Dispose();

        foreach (var secondaryStream in SecondaryStreams.Values) {
            secondaryStream?.Dispose();
        }
    }
}
using System.Text;

namespace Caxivitual.Lunacub.Compilation;

public readonly record struct Tag(uint Value) {
    public string AsAsciiString {
        get {
            unsafe {
                uint value = Value;
                return Encoding.ASCII.GetString(new ReadOnlySpan<byte>(&value, sizeof(uint)));
            }
        }
    }

    public static implicit operator Tag(uint value) => new(value);
    public static implicit operator uint(Tag value) => value.Value;
}
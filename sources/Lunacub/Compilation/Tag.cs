using System.Text;

namespace Caxivitual.Lunacub.Compilation;

public readonly record struct Tag {
    private readonly uint _value;
    
    public string AsAsciiString {
        get {
            unsafe {
                uint value = _value;
                return Encoding.ASCII.GetString(new ReadOnlySpan<byte>(&value, sizeof(uint)));
            }
        }
    }

    public string AsHexString {
        get {
            unsafe {
                uint value = _value;
                return Convert.ToHexString(new ReadOnlySpan<byte>(&value, sizeof(uint)));
            }
        }
    }

    public ReadOnlySpan<byte> AsSpan => MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(in this, 1));

    public uint AsUInt32LittleEndian => BitConverter.IsLittleEndian ? _value : BinaryPrimitives.ReverseEndianness(_value);
    
    public Tag(uint value) {
        _value = value;
    }

    public Tag(ReadOnlySpan<byte> name) {
        if (name.Length != 4) {
            throw new ArgumentException(ExceptionMessages.Expected4CharactersTagName, nameof(name));
        }

        _value = MemoryMarshal.Read<uint>(name);
    }
    
    public static implicit operator uint(Tag value) => value._value;
}
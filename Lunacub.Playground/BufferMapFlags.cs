namespace Lunacub.Playground;

[Flags]
public enum BufferMapFlags {
    Read = 1,
    Write = 2,
    Persistent = 4,
    PreserveData = 8,
}
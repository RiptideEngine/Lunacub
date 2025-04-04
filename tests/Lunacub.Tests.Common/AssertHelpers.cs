using Xunit.Abstractions;

namespace Caxivitual.Lunacub.Tests.Common;

public sealed class AssertHelpers {
    public static void RedirectConsoleOutput(ITestOutputHelper output) {
        Console.SetOut(new OutputWriter(output));
    }
    
    private class OutputWriter(ITestOutputHelper output) : TextWriter {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(object? value) {
            output.WriteLine(value?.ToString() ?? string.Empty);
        }

        public override void WriteLine(string? message) {
            output.WriteLine(message ?? string.Empty);
        }
        
        public override void WriteLine(ReadOnlySpan<char> buffer) {
            output.WriteLine(buffer.ToString());
        }

        public override void WriteLine(StringBuilder? value) {
            output.WriteLine(value?.ToString() ?? string.Empty);
        }

        public override void WriteLine(string format, object? arg0) {
            output.WriteLine(format, arg0);
        }

        public override void WriteLine(string format, object? arg0, object? arg1) {
            output.WriteLine(format, arg0, arg1);
        }

        public override void WriteLine(string format, object? arg0, object? arg1, object? arg2) {
            output.WriteLine(format, arg0, arg1, arg2);
        }
        
        public override void WriteLine(string format, params object?[] args) {
            output.WriteLine(format, args);
        }
        
        public override void WriteLine(string format, params ReadOnlySpan<object?> arg) {
            output.WriteLine(format, arg.ToArray());
        }

        public override void WriteLine(bool value) {
            output.WriteLine(value.ToString());
        }

        public override void WriteLine(uint value) {
            output.WriteLine(value.ToString());
        }

        public override void WriteLine(int value) {
            output.WriteLine(value.ToString());
        }
        
        public override void WriteLine(ulong value) {
            output.WriteLine(value.ToString());
        }

        public override void WriteLine(long value) {
            output.WriteLine(value.ToString());
        }

        public override void WriteLine(char value) {
            output.WriteLine(value.ToString());
        }

        public override void WriteLine(char[]? buffer) {
            output.WriteLine(buffer == null ? string.Empty : new(buffer));
        }

        public override void WriteLine(char[] buffer, int index, int count) {
            output.WriteLine(new(buffer, index, count));
        }

        public override void WriteLine(float value) {
            output.WriteLine(value.ToString(FormatProvider));
        }

        public override void WriteLine(double value) {
            output.WriteLine(value.ToString(FormatProvider));
        }
        
        public override void WriteLine(decimal value) {
            output.WriteLine(value.ToString(FormatProvider));
        }
    }
}
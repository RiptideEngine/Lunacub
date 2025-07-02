using System.Buffers;

namespace Caxivitual.Lunacub.Importing.Extensions;

internal static class StreamExtensions {
    private const int DefaultBufferSize = 81920;
    
    public static int CopyTo(this Stream source, Stream destination, int amount, int bufferSize = DefaultBufferSize) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        
        int oldAmount = amount;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        
        try {
            while (amount > 0) {
                int amountToRead = int.Min(buffer.Length, amount);
                int readAmount = source.Read(buffer, 0, amountToRead);

                if (readAmount == 0) break;
                
                destination.Write(buffer, 0, readAmount);
                amount -= readAmount;
            }
        } finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return oldAmount - amount;
    }
    
    public static async ValueTask<int> CopyToAsync(
        this Stream source,
        Stream destination,
        int amount,
        CancellationToken token,
        int bufferSize = DefaultBufferSize
    ) {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        
        int oldAmount = amount;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

        try {
            while (amount > 0) {
                token.ThrowIfCancellationRequested();
                
                int amountToRead = int.Min(buffer.Length, amount);
                int readAmount = await source.ReadAsync(buffer.AsMemory(0, amountToRead), token);

                if (readAmount == 0) break;
                
                await destination.WriteAsync(buffer.AsMemory(0, readAmount), token);
                amount -= readAmount;
            }
        } finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return oldAmount - amount;
    }
}
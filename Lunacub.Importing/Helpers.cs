using System.Buffers;

namespace Caxivitual.Lunacub.Importing;

internal static class Helpers {
    public static int CopyTo(this Stream source, Stream destination, int amount, int bufferSize = 4096) {
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
}
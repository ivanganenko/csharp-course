using System.Buffers;

namespace Module12.Tasks;

public static class StreamChecksum
{
    public static long SumBytes(Stream stream, ArrayPool<byte> pool, int bufferSize = 4096)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(pool);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        // Rent вместо new byte[]: при частых вызовах буферы переиспользуются, GC работает меньше.
        // Особенно важно для буферов >= 85 000 байт — они попали бы в LOH.
        var buffer = pool.Rent(bufferSize);
        try
        {
            long sum = 0;
            int read;
            // Читаем не больше bufferSize: массив из пула может быть длиннее.
            // И суммируем только первые read байт — в «хвосте» массива лежит мусор от прошлых владельцев.
            while ((read = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                for (var i = 0; i < read; i++)
                {
                    sum += buffer[i];
                }
            }

            return sum;
        }
        finally
        {
            // finally гарантирует возврат даже при исключении — иначе пул «протечёт»
            // и будет постоянно создавать новые массивы.
            pool.Return(buffer);
        }
    }
}

using System.Buffers;

namespace Module12.Tasks;

/// <summary>
/// Задача 12.4 — буфер из <see cref="ArrayPool{T}"/> вместо new byte[] на каждый вызов.
/// </summary>
public static class StreamChecksum
{
    /// <summary>
    /// Читает поток до конца порциями по <paramref name="bufferSize"/> байт и возвращает сумму всех байтов
    /// (каждый байт — число 0..255, сумма в long).
    /// Буфер берётся через pool.Rent(bufferSize) и ОБЯЗАТЕЛЬНО возвращается через pool.Return —
    /// даже если чтение потока бросило исключение (исключение при этом пробрасывается дальше).
    /// Внимание: Rent может вернуть массив ДЛИННЕЕ запрошенного и с «мусором» внутри —
    /// учитывай только реально прочитанные байты.
    /// stream или pool == null → ArgumentNullException; bufferSize &lt;= 0 → ArgumentOutOfRangeException.
    /// </summary>
    public static long SumBytes(Stream stream, ArrayPool<byte> pool, int bufferSize = 4096)
    {
        throw new NotImplementedException();
    }
}

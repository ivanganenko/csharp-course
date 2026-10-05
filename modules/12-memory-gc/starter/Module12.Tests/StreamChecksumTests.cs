using System.Buffers;
using Module12.Tasks;

namespace Module12.Tests;

public class StreamChecksumTests
{
    // Пул-шпион: считает Rent/Return и специально выдаёт массивы ДЛИННЕЕ запрошенного,
    // заполненные «мусором» 0xFF — как настоящий ArrayPool, который отдаёт переиспользованные массивы.
    private sealed class CountingPool : ArrayPool<byte>
    {
        public int Rented { get; private set; }
        public int Returned { get; private set; }

        public override byte[] Rent(int minimumLength)
        {
            Rented++;
            var array = new byte[minimumLength + 7];
            Array.Fill(array, (byte)0xFF);
            return array;
        }

        public override void Return(byte[] array, bool clearArray = false)
        {
            Returned++;
        }
    }

    private static MemoryStream StreamOf(params byte[] bytes) => new(bytes);

    [Fact]
    public void SumBytes_SmallStream_ReturnsSum()
    {
        var pool = new CountingPool();

        var sum = StreamChecksum.SumBytes(StreamOf(1, 2, 3, 4, 5, 6, 7, 8, 9, 10), pool);

        Assert.Equal(55L, sum);
    }

    [Fact]
    public void SumBytes_SmallBuffer_ReadsInSeveralChunks()
    {
        var pool = new CountingPool();

        var sum = StreamChecksum.SumBytes(StreamOf(1, 2, 3, 4, 5, 6, 7, 8, 9, 10), pool, bufferSize: 3);

        Assert.Equal(55L, sum);
    }

    [Fact]
    public void SumBytes_PoolReturnsLongerArrayWithGarbage_CountsOnlyReadBytes()
    {
        var pool = new CountingPool();

        var sum = StreamChecksum.SumBytes(StreamOf(1, 2, 3), pool, bufferSize: 2);

        Assert.Equal(6L, sum);
    }

    [Fact]
    public void SumBytes_EmptyStream_ReturnsZero()
    {
        var pool = new CountingPool();

        Assert.Equal(0L, StreamChecksum.SumBytes(new MemoryStream(), pool));
    }

    [Fact]
    public void SumBytes_RentsOnceAndReturnsBuffer()
    {
        var pool = new CountingPool();

        StreamChecksum.SumBytes(StreamOf(5, 5), pool, bufferSize: 1);

        Assert.Equal(1, pool.Rented);
        Assert.Equal(1, pool.Returned);
    }

    [Fact]
    public void SumBytes_StreamThrows_BufferIsStillReturned()
    {
        var pool = new CountingPool();
        var broken = StreamOf(1, 2, 3);
        broken.Dispose(); // чтение из закрытого потока бросит исключение

        Assert.ThrowsAny<Exception>(() => StreamChecksum.SumBytes(broken, pool));
        Assert.Equal(pool.Rented, pool.Returned);
    }

    [Fact]
    public void SumBytes_LargeStream_UsesLongWithoutOverflow()
    {
        var data = new byte[10_000];
        Array.Fill(data, (byte)255);

        var sum = StreamChecksum.SumBytes(new MemoryStream(data), ArrayPool<byte>.Shared, bufferSize: 1024);

        Assert.Equal(2_550_000L, sum);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SumBytes_InvalidBufferSize_ThrowsArgumentOutOfRange(int bufferSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => StreamChecksum.SumBytes(StreamOf(1), new CountingPool(), bufferSize));
    }

    [Fact]
    public void SumBytes_NullArguments_ThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => StreamChecksum.SumBytes(null!, new CountingPool()));
        Assert.Throws<ArgumentNullException>(() => StreamChecksum.SumBytes(StreamOf(1), null!));
    }
}

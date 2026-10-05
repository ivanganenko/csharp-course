using Module07.Tasks;

namespace Module07.Tests;

public class AppConfigTests
{
    [Fact]
    public void Parse_SimplePairs_ReadsStringsAndInts()
    {
        var config = AppConfig.Parse("host=localhost\nport=8080");

        Assert.Equal(2, config.Count);
        Assert.Equal("localhost", config.GetString("host"));
        Assert.Equal(8080, config.GetInt("port"));
    }

    [Fact]
    public void Parse_SkipsCommentsBlankLinesAndHandlesCrLf()
    {
        var config = AppConfig.Parse("# settings\r\n\r\n  name =  Demo App  \r\n   \r\n# end");

        Assert.Equal(1, config.Count);
        Assert.Equal("Demo App", config.GetString("name"));
    }

    [Fact]
    public void Parse_ValueMayContainEquals_KeysAreCaseInsensitive()
    {
        var config = AppConfig.Parse("conn=Server=db;User=app\nPort=1");

        Assert.Equal("Server=db;User=app", config.GetString("CONN"));
        Assert.Equal(1, config.GetInt("port"));
    }

    [Theory]
    [InlineData("a=1\nno equals sign", 2)]
    [InlineData("=value", 1)]
    [InlineData("# comment\nx=1\nX=2", 3)]
    public void Parse_InvalidLine_ThrowsWithLineNumber(string text, int expectedLine)
    {
        var ex = Assert.Throws<ConfigParseException>(() => AppConfig.Parse(text));

        Assert.Equal(expectedLine, ex.LineNumber);
        Assert.False(string.IsNullOrWhiteSpace(ex.Message));
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => AppConfig.Parse(null!));
    }

    [Fact]
    public void Parse_EmptyText_GivesEmptyConfig()
    {
        Assert.Equal(0, AppConfig.Parse("").Count);
    }

    [Fact]
    public void GetString_MissingKey_ThrowsKeyNotFound()
    {
        var config = AppConfig.Parse("a=1");

        Assert.Throws<KeyNotFoundException>(() => config.GetString("b"));
        Assert.Throws<KeyNotFoundException>(() => config.GetInt("b"));
    }

    [Fact]
    public void GetInt_NotANumber_WrapsFormatExceptionWithLineNumber()
    {
        var config = AppConfig.Parse("# c\nport=abc");

        var ex = Assert.Throws<ConfigParseException>(() => config.GetInt("port"));

        Assert.Equal(2, ex.LineNumber);
        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public void GetInt_TooBig_WrapsOverflowException()
    {
        var config = AppConfig.Parse("size=99999999999");

        var ex = Assert.Throws<ConfigParseException>(() => config.GetInt("size"));

        Assert.Equal(1, ex.LineNumber);
        Assert.IsType<OverflowException>(ex.InnerException);
    }

    [Fact]
    public void TryGetInt_NeverThrows()
    {
        var config = AppConfig.Parse("port=8080\nname=demo");

        Assert.True(config.TryGetInt("port", out var port));
        Assert.Equal(8080, port);
        Assert.False(config.TryGetInt("name", out var notNumber));
        Assert.Equal(0, notNumber);
        Assert.False(config.TryGetInt("missing", out var missing));
        Assert.Equal(0, missing);
    }
}

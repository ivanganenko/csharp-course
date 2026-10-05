using Module07.Tasks;

namespace Module07.Tests;

public class ResultTests
{
    [Fact]
    public void Success_HasValueAndNoError()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Throws<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Failure_HasErrorAndNoValue()
    {
        var result = Result<int>.Failure("boom");

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("boom", result.Error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Failure_BlankError_Throws(string? error)
    {
        Assert.ThrowsAny<ArgumentException>(() => Result<int>.Failure(error!));
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result<int>.Success(21).Map(x => x * 2).Map(x => $"#{x}");

        Assert.True(result.IsSuccess);
        Assert.Equal("#42", result.Value);
    }

    [Fact]
    public void Map_OnFailure_KeepsErrorAndDoesNotCallFunction()
    {
        bool called = false;

        var result = Result<int>.Failure("no data").Map(x => { called = true; return x * 2; });

        Assert.True(result.IsFailure);
        Assert.Equal("no data", result.Error);
        Assert.False(called);
    }

    [Fact]
    public void Bind_ChainsOperationsThatCanFail()
    {
        static Result<string> Classify(int age) =>
            age >= 18 ? Result<string>.Success("adult") : Result<string>.Failure("too young");

        Assert.Equal("adult", AgeParser.Parse("42").Bind(Classify).Value);
        Assert.Equal("too young", AgeParser.Parse("12").Bind(Classify).Error);
        Assert.Equal("Age must be a number", AgeParser.Parse("abc").Bind(Classify).Error);
    }

    [Fact]
    public void Match_CallsMatchingBranch()
    {
        var ok = Result<int>.Success(5).Match(v => $"value {v}", e => $"error {e}");
        var bad = Result<int>.Failure("oops").Match(v => $"value {v}", e => $"error {e}");

        Assert.Equal("value 5", ok);
        Assert.Equal("error oops", bad);
    }

    [Fact]
    public void GetValueOrDefault_ReturnsFallbackOnFailure()
    {
        Assert.Equal(5, Result<int>.Success(5).GetValueOrDefault(-1));
        Assert.Equal(-1, Result<int>.Failure("x").GetValueOrDefault(-1));
    }

    [Fact]
    public void NullFunctions_Throw()
    {
        var result = Result<int>.Success(1);

        Assert.Throws<ArgumentNullException>(() => result.Map<int>(null!));
        Assert.Throws<ArgumentNullException>(() => result.Bind<int>(null!));
    }

    [Theory]
    [InlineData("42", 42)]
    [InlineData(" 7 ", 7)]
    [InlineData("0", 0)]
    [InlineData("150", 150)]
    public void AgeParser_ValidInput_Success(string input, int expected)
    {
        var result = AgeParser.Parse(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData(null, "Age is required")]
    [InlineData("", "Age is required")]
    [InlineData("   ", "Age is required")]
    [InlineData("abc", "Age must be a number")]
    [InlineData("4.5", "Age must be a number")]
    [InlineData("99999999999", "Age must be a number")]
    [InlineData("151", "Age must be between 0 and 150")]
    [InlineData("-1", "Age must be between 0 and 150")]
    public void AgeParser_InvalidInput_FailureWithMessage(string? input, string expectedError)
    {
        var result = AgeParser.Parse(input);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }
}

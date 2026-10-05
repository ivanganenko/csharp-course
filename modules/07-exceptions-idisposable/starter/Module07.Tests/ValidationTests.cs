using Module07.Tasks;

namespace Module07.Tests;

public class ValidationTests
{
    private static RegistrationRequest Valid() =>
        new() { Username = "anna_dev", Email = "anna@example.com", Age = 25 };

    [Fact]
    public void ValidationException_StoresPropertyNameMessageAndInner()
    {
        var inner = new FormatException("bad number");

        var ex = new ValidationException("Age", "Age is invalid", inner);

        Assert.Equal("Age", ex.PropertyName);
        Assert.Equal("Age is invalid", ex.Message);
        Assert.Same(inner, ex.InnerException);
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void ValidationException_BlankPropertyName_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() => new ValidationException(" ", "message"));
    }

    [Fact]
    public void Validate_ValidRequest_DoesNotThrow()
    {
        var ex = Record.Exception(() => RegistrationValidator.Validate(Valid()));

        Assert.Null(ex);
    }

    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RegistrationValidator.Validate(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("this_name_is_way_too_long")]
    [InlineData("anna dev")]
    [InlineData("anna-dev")]
    [InlineData("анна")]
    public void Validate_InvalidUsername_ThrowsWithPropertyName(string? username)
    {
        var request = new RegistrationRequest { Username = username, Email = "a@b.c", Age = 20 };

        var ex = Assert.Throws<ValidationException>(() => RegistrationValidator.Validate(request));

        Assert.Equal("Username", ex.PropertyName);
        Assert.False(string.IsNullOrWhiteSpace(ex.Message));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("anna.example.com")]
    [InlineData("@example.com")]
    [InlineData("anna@")]
    [InlineData("anna@@example.com")]
    [InlineData("an na@example.com")]
    public void Validate_InvalidEmail_ThrowsWithPropertyName(string? email)
    {
        var request = new RegistrationRequest { Username = "anna", Email = email, Age = 20 };

        var ex = Assert.Throws<ValidationException>(() => RegistrationValidator.Validate(request));

        Assert.Equal("Email", ex.PropertyName);
    }

    [Theory]
    [InlineData(13)]
    [InlineData(121)]
    [InlineData(-1)]
    public void Validate_InvalidAge_ThrowsWithPropertyName(int age)
    {
        var request = new RegistrationRequest { Username = "anna", Email = "a@b.c", Age = age };

        var ex = Assert.Throws<ValidationException>(() => RegistrationValidator.Validate(request));

        Assert.Equal("Age", ex.PropertyName);
    }

    [Theory]
    [InlineData(RegistrationValidator.MinAge)]
    [InlineData(RegistrationValidator.MaxAge)]
    public void Validate_BoundaryAge_IsValid(int age)
    {
        var request = new RegistrationRequest { Username = "abc", Email = "a@b", Age = age };

        Assert.Null(Record.Exception(() => RegistrationValidator.Validate(request)));
    }

    [Fact]
    public void Validate_SeveralErrors_ReportsFirstInOrder()
    {
        var request = new RegistrationRequest { Username = "x", Email = "bad", Age = 5 };

        var ex = Assert.Throws<ValidationException>(() => RegistrationValidator.Validate(request));

        Assert.Equal("Username", ex.PropertyName);
    }

    [Fact]
    public void GetErrors_CollectsAllErrorsWithoutThrowing()
    {
        var all = RegistrationValidator.GetErrors(new RegistrationRequest { Username = "x", Email = "bad", Age = 5 });
        var none = RegistrationValidator.GetErrors(Valid());
        var onlyAge = RegistrationValidator.GetErrors(new RegistrationRequest { Username = "anna", Email = "a@b.c", Age = 200 });

        Assert.Equal("Username,Email,Age", string.Join(",", all));
        Assert.Empty(none);
        Assert.Equal("Age", string.Join(",", onlyAge));
        Assert.Throws<ArgumentNullException>(() => RegistrationValidator.GetErrors(null!));
    }
}

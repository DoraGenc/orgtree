namespace OrgTree.Core.Tests;

public class EmployeeTests
{
    [Theory]
    [InlineData("+4917212345678", "+49 172 12345678")]
    [InlineData("+49 172 12345678", "+49 172 12345678")]
    [InlineData("+49 (0)172-123 45678", "+49 172 12345678")]
    [InlineData("+493012345678", "+49 30 12345678")]
    [InlineData("  +4917212345678  ", "+49 172 12345678")]
    public void PhoneNumberIsStoredInInternationalFormat(string input, string expected)
    {
        var employee = new Employee { FirstName = "Anna", LastName = "Müller", PhoneNumber = input };

        Assert.Equal(expected, employee.PhoneNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EmptyPhoneNumberIsStoredAsNull(string? input)
    {
        var employee = new Employee { FirstName = "Anna", LastName = "Müller", PhoneNumber = input };

        Assert.Null(employee.PhoneNumber);
    }

    [Theory]
    [InlineData("0172 12345678")]
    [InlineData("hallo")]
    [InlineData("+49 999 123")]
    [InlineData("+49 172 1")]
    public void InvalidPhoneNumberThrows(string input)
    {
        var employee = new Employee { FirstName = "Anna", LastName = "Müller" };

        Assert.Throws<ArgumentException>(() => employee.PhoneNumber = input);
    }

    [Theory]
    [InlineData("+49 172 12345678", true)]
    [InlineData("+1 212 555 0100", true)]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("0172 12345678", false)]
    [InlineData("hallo", false)]
    public void IsValidPhoneNumberChecksInternationalFormat(string? input, bool expected)
    {
        Assert.Equal(expected, Employee.IsValidPhoneNumber(input));
    }

    [Fact]
    public void TryFormatPhoneNumberReturnsFormattedNumber()
    {
        var success = Employee.TryFormatPhoneNumber("+4917212345678", out var formatted);

        Assert.True(success);
        Assert.Equal("+49 172 12345678", formatted);
    }

    [Fact]
    public void TryFormatPhoneNumberReturnsNullForInvalidNumber()
    {
        var success = Employee.TryFormatPhoneNumber("hallo", out var formatted);

        Assert.False(success);
        Assert.Null(formatted);
    }

    [Fact]
    public void NamesAreFormattedForDisplayAndSorting()
    {
        var employee = new Employee { FirstName = "Anna", LastName = "Müller" };

        Assert.Equal("Anna Müller", employee.FullName);
        Assert.Equal("Müller, Anna", employee.DisplayName);
    }
}

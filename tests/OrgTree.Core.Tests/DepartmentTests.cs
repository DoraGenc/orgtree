namespace OrgTree.Core.Tests;

public class DepartmentTests
{
    [Theory]
    [InlineData("#FFF")]
    [InlineData("#55aaff")]
    [InlineData("#55AAFF")]
    [InlineData("#FF55AAFF")]
    [InlineData(null)]
    public void ValidColorHexIsStored(string? input)
    {
        var department = new Department { Name = "Vertrieb", ColorHex = input };

        Assert.Equal(input, department.ColorHex);
    }

    [Theory]
    [InlineData("")]
    [InlineData("55AAFF")]
    [InlineData("#GGG")]
    [InlineData("#12345")]
    [InlineData("rot")]
    public void InvalidColorHexThrows(string input)
    {
        var department = new Department { Name = "Vertrieb" };

        Assert.Throws<ArgumentException>(() => department.ColorHex = input);
    }

    [Theory]
    [InlineData("#ABC", true)]
    [InlineData(null, true)]
    [InlineData("ABC", false)]
    [InlineData("#ABCD", false)]
    public void IsValidColorHexChecksFormat(string? input, bool expected)
    {
        Assert.Equal(expected, Department.IsValidColorHex(input));
    }
}

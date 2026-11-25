using OrgTree.App.ViewModels;

namespace OrgTree.App.Tests.ViewModels;

public class OrgChartViewModelTests : DatabaseTest
{
    [Fact]
    public void EmptyDatabaseShowsEmptyState()
    {
        var viewModel = new OrgChartViewModel();

        viewModel.Load();

        Assert.Empty(viewModel.Roots);
        Assert.True(viewModel.IsEmpty);
    }

    [Fact]
    public void DepartmentWithoutParentOrChildrenIsNotShown()
    {
        AddDepartment("Vertrieb");
        var viewModel = new OrgChartViewModel();

        viewModel.Load();

        Assert.Empty(viewModel.Roots);
        Assert.True(viewModel.IsEmpty);
    }

    [Fact]
    public void ParentAndChildDepartmentsAreShownAsTree()
    {
        var rootId = AddDepartment("Geschäftsführung");
        var salesId = AddDepartment("Vertrieb", parentDepartmentId: rootId);
        AddDepartment("Vertrieb Nord", parentDepartmentId: salesId);
        AddDepartment("IT");
        var viewModel = new OrgChartViewModel();

        viewModel.Load();

        var root = Assert.Single(viewModel.Roots);
        Assert.Equal("Geschäftsführung", root.Name);
        Assert.False(root.HasParent);
        Assert.True(root.HasChildren);

        var sales = Assert.Single(root.Children);
        Assert.Equal("Vertrieb", sales.Name);
        Assert.True(sales.HasParent);

        var north = Assert.Single(sales.Children);
        Assert.Equal("Vertrieb Nord", north.Name);
        Assert.False(north.HasChildren);
        Assert.False(viewModel.IsEmpty);
    }

    [Fact]
    public void ChildDepartmentsAreSortedByName()
    {
        var rootId = AddDepartment("Geschäftsführung");
        AddDepartment("Vertrieb", parentDepartmentId: rootId);
        AddDepartment("Einkauf", parentDepartmentId: rootId);
        AddDepartment("IT", parentDepartmentId: rootId);
        var viewModel = new OrgChartViewModel();

        viewModel.Load();

        Assert.Equal(["Einkauf", "IT", "Vertrieb"], viewModel.Roots.Single().Children.Select(node => node.Name));
    }

    [Fact]
    public void BoxShowsLeaderAsFirstNameThenLastName()
    {
        var rootId = AddDepartment("Geschäftsführung");
        var salesId = AddDepartment("Vertrieb", parentDepartmentId: rootId);
        AddEmployee("Müller", "Anna", departmentId: rootId, isLeader: true);
        var viewModel = new OrgChartViewModel();

        viewModel.Load();

        var root = viewModel.Roots.Single();
        Assert.Equal("Anna Müller", root.LeaderName);
        Assert.Null(root.Children.Single().LeaderName);
    }

    [Fact]
    public void BoxUsesDepartmentColorOrDefault()
    {
        var rootId = AddDepartment("Geschäftsführung", colorHex: "#55AAFF");
        AddDepartment("Vertrieb", parentDepartmentId: rootId);
        var viewModel = new OrgChartViewModel();

        viewModel.Load();

        var root = viewModel.Roots.Single();
        Assert.Equal("#55AAFF", root.BoxColor);
        Assert.Equal(DisplayTexts.DefaultDepartmentColor, root.Children.Single().BoxColor);
    }

    [Fact]
    public void ConnectorLinesJoinSiblingsButNotTheOuterEnds()
    {
        var rootId = AddDepartment("Geschäftsführung");
        AddDepartment("A", parentDepartmentId: rootId);
        AddDepartment("B", parentDepartmentId: rootId);
        AddDepartment("C", parentDepartmentId: rootId);
        var viewModel = new OrgChartViewModel();

        viewModel.Load();

        var children = viewModel.Roots.Single().Children;
        Assert.Equal([false, true, true], children.Select(node => node.ShowLeftLine));
        Assert.Equal([true, true, false], children.Select(node => node.ShowRightLine));
    }

    [Fact]
    public void BoxesStartExpanded()
    {
        var rootId = AddDepartment("Geschäftsführung");
        AddDepartment("Vertrieb", parentDepartmentId: rootId);
        var viewModel = new OrgChartViewModel();

        viewModel.Load();

        Assert.True(viewModel.Roots.Single().IsExpanded);
    }

    [Fact]
    public void LoadingTwiceDoesNotDuplicateBoxes()
    {
        var rootId = AddDepartment("Geschäftsführung");
        AddDepartment("Vertrieb", parentDepartmentId: rootId);
        var viewModel = new OrgChartViewModel();

        viewModel.Load();
        viewModel.Load();

        Assert.Single(viewModel.Roots);
    }
}

using Module11.Tasks;

namespace Module11.Tests;

public class HrQueriesTests
{
    [Fact]
    public void EmployeesWithDepartments_InnerJoin_ExcludesUnmatched()
    {
        var result = HrQueries.EmployeesWithDepartments(TestData.Employees(), TestData.Departments());

        Assert.Equal(
            new[] { ("Alice", "IT"), ("Bob", "IT"), ("Carol", "Sales") },
            result);
    }

    [Fact]
    public void AllEmployeesWithDepartments_LeftJoin_KeepsEveryone()
    {
        var result = HrQueries.AllEmployeesWithDepartments(TestData.Employees(), TestData.Departments());

        Assert.Equal(5, result.Count);
        Assert.Equal(("Alice", (string?)"IT"), result[0]);
        Assert.Equal(("Carol", (string?)"Sales"), result[2]);
        Assert.Equal("Dave", result[3].Employee);
        Assert.Null(result[3].Department);
        Assert.Equal("Eve", result[4].Employee);
        Assert.Null(result[4].Department);
    }

    [Fact]
    public void DepartmentSummaries_IncludesEmptyDepartments_SortedByName()
    {
        var result = HrQueries.DepartmentSummaries(TestData.Employees(), TestData.Departments());

        Assert.Equal(
            new[]
            {
                new DepartmentSummary("IT", 2, 2500m),
                new DepartmentSummary("Legal", 0, 0m),
                new DepartmentSummary("Sales", 1, 2500m),
            },
            result);
    }

    [Fact]
    public void DepartmentSummaries_NoEmployees_AllZero()
    {
        var result = HrQueries.DepartmentSummaries(new List<Employee>(), TestData.Departments());

        Assert.All(result, summary =>
        {
            Assert.Equal(0, summary.Headcount);
            Assert.Equal(0m, summary.AverageSalary);
        });
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void EmployeeManagerPairs_SelfJoin()
    {
        var result = HrQueries.EmployeeManagerPairs(TestData.Employees());

        Assert.Equal(
            new[] { ("Bob", "Alice"), ("Carol", "Alice"), ("Dave", "Carol") },
            result);
    }

    [Fact]
    public void EmployeeManagerPairs_UnknownManagerId_IsSkipped()
    {
        var employees = new List<Employee>
        {
            new(1, "Solo", null, 100m, 42),
        };

        Assert.Empty(HrQueries.EmployeeManagerPairs(employees));
    }
}

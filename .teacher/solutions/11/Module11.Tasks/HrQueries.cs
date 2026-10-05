namespace Module11.Tasks;

public static class HrQueries
{
    public static IReadOnlyList<(string Employee, string Department)> EmployeesWithDepartments(
        IEnumerable<Employee> employees,
        IEnumerable<Department> departments)
    {
        // Join внутри строит хэш-таблицу по ключам второй последовательности: O(n + m),
        // а не O(n * m), как было бы с вложенным Where.
        // Ключи: int? и int? — сотрудник с DepartmentId = null не совпадёт ни с одним отделом.
        return employees
            .Join(
                departments,
                employee => employee.DepartmentId,
                department => (int?)department.Id,
                (employee, department) => (Employee: employee.Name, Department: department.Name))
            .OrderBy(pair => pair.Employee, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<(string Employee, string? Department)> AllEmployeesWithDepartments(
        IEnumerable<Employee> employees,
        IEnumerable<Department> departments)
    {
        // Left join в LINQ (до .NET 10) — это GroupJoin + SelectMany + DefaultIfEmpty.
        // Query syntax здесь заметно читаемее method syntax.
        // (В .NET 10 появился оператор LeftJoin, но классическую форму спрашивают на собеседованиях.)
        var query =
            from employee in employees
            join department in departments on employee.DepartmentId equals (int?)department.Id into matches
            from department in matches.DefaultIfEmpty()
            select (Employee: employee.Name, Department: department?.Name);

        // orderby в query syntax не принимает компаратор (использует culture-sensitive сравнение строк),
        // поэтому сортировку с StringComparer.Ordinal делаем в method syntax — синтаксисы можно смешивать.
        return query.OrderBy(pair => pair.Employee, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<DepartmentSummary> DepartmentSummaries(
        IEnumerable<Employee> employees,
        IEnumerable<Department> departments)
    {
        // GroupJoin от отделов: КАЖДЫЙ отдел получает свою (возможно, пустую) группу сотрудников.
        // Это и отличает его от GroupBy по сотрудникам, где пустые отделы просто не появились бы.
        return departments
            .GroupJoin(
                employees,
                department => (int?)department.Id,
                employee => employee.DepartmentId,
                (department, staff) =>
                {
                    // Материализуем группу: дальше по ней два прохода (Count и Average).
                    List<Employee> members = staff.ToList();
                    // Average на пустой последовательности бросает InvalidOperationException —
                    // поэтому для пустого отдела явно 0.
                    decimal average = members.Count == 0 ? 0m : members.Average(e => e.Salary);
                    return new DepartmentSummary(department.Name, members.Count, average);
                })
            .OrderBy(summary => summary.Department, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<(string Employee, string Manager)> EmployeeManagerPairs(IEnumerable<Employee> employees)
    {
        // Self-join: последовательность соединяется сама с собой. Материализуем один раз,
        // чтобы не перечислять источник дважды.
        List<Employee> all = employees.ToList();

        return all
            .Join(
                all,
                employee => employee.ManagerId,
                manager => (int?)manager.Id,
                (employee, manager) => (Employee: employee.Name, Manager: manager.Name))
            .OrderBy(pair => pair.Employee, StringComparer.Ordinal)
            .ToList();
    }
}

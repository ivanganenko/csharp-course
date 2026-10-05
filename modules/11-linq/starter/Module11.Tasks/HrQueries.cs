namespace Module11.Tasks;

/// <summary>
/// Задача 11.3 — соединения (joins): Join, GroupJoin, left join, self-join.
/// Сортировка строк — ordinal. Тесты: HrQueriesTests.
/// </summary>
public static class HrQueries
{
    /// <summary>
    /// Inner join: пары (имя сотрудника, название отдела). Сотрудники без отдела или с несуществующим
    /// DepartmentId не попадают. Сортировка по имени сотрудника.
    /// </summary>
    public static IReadOnlyList<(string Employee, string Department)> EmployeesWithDepartments(
        IEnumerable<Employee> employees,
        IEnumerable<Department> departments)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Left join: ВСЕ сотрудники; Department = null, если отдела нет или он не найден.
    /// Сортировка по имени сотрудника.
    /// </summary>
    public static IReadOnlyList<(string Employee, string? Department)> AllEmployeesWithDepartments(
        IEnumerable<Employee> employees,
        IEnumerable<Department> departments)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Сводка по КАЖДОМУ отделу (включая отделы без сотрудников): численность и средняя зарплата
    /// (0 для пустого отдела — осторожно, Average на пустой последовательности бросает исключение).
    /// Сортировка по названию отдела.
    /// </summary>
    public static IReadOnlyList<DepartmentSummary> DepartmentSummaries(
        IEnumerable<Employee> employees,
        IEnumerable<Department> departments)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Self-join: пары (сотрудник, его руководитель) для сотрудников, у которых ManagerId указывает
    /// на существующего сотрудника. Сортировка по имени сотрудника.
    /// </summary>
    public static IReadOnlyList<(string Employee, string Manager)> EmployeeManagerPairs(IEnumerable<Employee> employees)
    {
        throw new NotImplementedException();
    }
}

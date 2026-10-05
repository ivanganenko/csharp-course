using Module11.Tasks;

namespace Module11.Tests;

/// <summary>
/// Общие вымышленные данные для тестов. Ожидаемые значения посчитаны вручную — см. комментарии.
/// </summary>
internal static class TestData
{
    //  Id  Customer  Date        Status     Lines                                    Total
    //  1   Alice     2025-01-05  Completed  Laptop 1×1000, Mouse 2×25                1050
    //  2   Bob       2025-01-20  Completed  Book 3×20                                60
    //  3   Alice     2025-02-02  Cancelled  Phone 1×500                              500
    //  4   Carol     2025-02-14  Completed  Mouse 1×25, Book 1×20                    45
    //  5   Bob       2025-02-28  New        Laptop 1×1000                            1000
    //  6   Alice     2025-03-10  Completed  Book 2×20, Pen 10×2                      60
    public static List<Order> Orders() =>
    [
        new(1, "Alice", new DateOnly(2025, 1, 5), OrderStatus.Completed,
            [new("Laptop", "Electronics", 1, 1000m), new("Mouse", "Electronics", 2, 25m)]),
        new(2, "Bob", new DateOnly(2025, 1, 20), OrderStatus.Completed,
            [new("Book", "Books", 3, 20m)]),
        new(3, "Alice", new DateOnly(2025, 2, 2), OrderStatus.Cancelled,
            [new("Phone", "Electronics", 1, 500m)]),
        new(4, "Carol", new DateOnly(2025, 2, 14), OrderStatus.Completed,
            [new("Mouse", "Electronics", 1, 25m), new("Book", "Books", 1, 20m)]),
        new(5, "Bob", new DateOnly(2025, 2, 28), OrderStatus.New,
            [new("Laptop", "Electronics", 1, 1000m)]),
        new(6, "Alice", new DateOnly(2025, 3, 10), OrderStatus.Completed,
            [new("Book", "Books", 2, 20m), new("Pen", "Stationery", 10, 2m)]),
    ];

    public static List<Department> Departments() =>
    [
        new(1, "IT"),
        new(2, "Sales"),
        new(3, "Legal"), // пустой отдел
    ];

    //  Id  Name   Dept  Salary  Manager
    //  1   Alice  1     3000    -
    //  2   Bob    1     2000    1
    //  3   Carol  2     2500    1
    //  4   Dave   -     1500    3
    //  5   Eve    99    1000    -       (несуществующий отдел)
    public static List<Employee> Employees() =>
    [
        new(1, "Alice", 1, 3000m, null),
        new(2, "Bob", 1, 2000m, 1),
        new(3, "Carol", 2, 2500m, 1),
        new(4, "Dave", null, 1500m, 3),
        new(5, "Eve", 99, 1000m, null),
    ];
}

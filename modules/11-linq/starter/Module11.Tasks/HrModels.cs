namespace Module11.Tasks;

// Модели отдела кадров для задачи 11.3. Готовы — менять не нужно.

public sealed record Department(int Id, string Name);

/// <summary>Сотрудник. DepartmentId и ManagerId могут быть null (нет отдела / нет руководителя).</summary>
public sealed record Employee(int Id, string Name, int? DepartmentId, decimal Salary, int? ManagerId);

/// <summary>Сводка по отделу: численность и средняя зарплата (0, если сотрудников нет).</summary>
public sealed record DepartmentSummary(string Department, int Headcount, decimal AverageSalary);

namespace Module17.Tasks;

/// <summary>Хранилище заказов (абстракция для DIP — задача 6).</summary>
public interface IOrderRepository
{
    /// <summary>
    /// Следующий номер заказа: "ORD-{yyyyMMdd}-{NNNN}", где NNNN — сквозной счётчик (0001, 0002, …)
    /// в пределах экземпляра хранилища. Вызывается только для успешно рассчитанного заказа.
    /// </summary>
    string NextOrderNumber(DateOnly orderDate);

    void Save(OrderResult order);

    OrderResult? FindByNumber(string orderNumber);
}

namespace Module17.Tasks;

// Builder: собирает сложный объект по шагам, валидирует и отдаёт готовый результат в Build().
// Fluent API (каждый метод возвращает this) читается как предложение и не требует конструктора на 5 параметров.
public sealed class OrderRequestBuilder
{
    private const string DefaultDelivery = "pickup";

    // Список, а не Dictionary: нужно сохранить порядок первого добавления.
    // Для заказа из пары строк линейный поиск дешевле и проще (KISS).
    private readonly List<OrderLine> _lines = new();
    private string? _email;
    private CustomerTier _tier = CustomerTier.Regular;
    private string _deliveryType = DefaultDelivery;
    private DateOnly? _orderDate;

    public OrderRequestBuilder ForCustomer(string email, CustomerTier tier = CustomerTier.Regular)
    {
        // Ошибки аргументов — сразу (fail fast), а «не хватает данных» — в Build.
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        _email = email;
        _tier = tier;
        return this;
    }

    public OrderRequestBuilder AddItem(string sku, int quantity = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var index = _lines.FindIndex(line => line.Sku == sku);
        if (index >= 0)
        {
            // OrderLine — immutable record: «изменяем» через with, создавая новую строку.
            _lines[index] = _lines[index] with { Quantity = _lines[index].Quantity + quantity };
        }
        else
        {
            _lines.Add(new OrderLine(sku, quantity));
        }

        return this;
    }

    public OrderRequestBuilder WithDelivery(string deliveryType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deliveryType);
        _deliveryType = deliveryType;
        return this;
    }

    public OrderRequestBuilder On(DateOnly orderDate)
    {
        _orderDate = orderDate;
        return this;
    }

    public OrderRequest Build()
    {
        if (_email is null)
        {
            throw new InvalidOperationException("Не задан клиент: вызовите ForCustomer");
        }

        if (_lines.Count == 0)
        {
            throw new InvalidOperationException("В заказе нет товаров: вызовите AddItem");
        }

        if (_orderDate is not { } orderDate)
        {
            throw new InvalidOperationException("Не задана дата заказа: вызовите On");
        }

        return new OrderRequest
        {
            CustomerEmail = _email,
            Tier = _tier,
            // ToArray — КОПИЯ: иначе созданный запрос делил бы список с builder'ом,
            // и следующий AddItem «тихо» изменил бы уже отданный заказ (aliasing bug).
            Lines = _lines.ToArray(),
            DeliveryType = _deliveryType,
            OrderDate = orderDate,
        };
    }
}

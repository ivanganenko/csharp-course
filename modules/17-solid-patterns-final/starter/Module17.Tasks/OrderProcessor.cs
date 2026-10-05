using System.Globalization;

namespace Module17.Tasks;

/// <summary>
/// ⚠️ LEGACY-КОД ДЛЯ РЕФАКТОРИНГА. Он РАБОТАЕТ (характеризационные тесты зелёные), но спроектирован плохо.
/// Публичный фасад, который нужно сохранить: конструктор без параметров, Process(OrderRequest), GetOrder(string).
/// Найди здесь нарушения SOLID и «запахи кода» (задача 1), затем вынеси части в отдельные классы (задачи 2–6).
/// </summary>
public class OrderProcessor
{
    private Dictionary<string, OrderResult> _orders = new Dictionary<string, OrderResult>();
    private int _counter = 0;

    public OrderProcessor()
    {
    }

    /// <summary>
    /// Задача 6 ⭐. Конструктор для внедрения зависимостей. Сейчас не реализован.
    /// После рефакторинга конструктор без параметров должен вызывать этот: this(стандартные реализации...).
    /// Любой аргумент null → ArgumentNullException.
    /// </summary>
    public OrderProcessor(
        IProductCatalog catalog,
        IDiscountPolicy discountPolicy,
        DeliveryMethodRegistry deliveryMethods,
        IOrderRepository repository,
        INotificationSender notificationSender)
    {
        throw new NotImplementedException();
    }

    public OrderResult Process(OrderRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        Console.WriteLine("[LOG] " + DateTime.Now + " Обработка заказа для " + request.CustomerEmail);

        // --- валидация ---
        if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains('@'))
        {
            throw new ArgumentException("Некорректный email");
        }

        if (request.Lines == null || request.Lines.Count == 0)
        {
            throw new ArgumentException("Заказ пуст");
        }

        // --- подсчёт суммы и веса ---
        decimal subtotal = 0;
        decimal weight = 0;
        foreach (var line in request.Lines)
        {
            if (line.Quantity <= 0)
            {
                throw new ArgumentException("Количество должно быть больше нуля: " + line.Sku);
            }

            if (!LegacyProductCatalog.Products.ContainsKey(line.Sku))
            {
                throw new ArgumentException("Неизвестный товар " + line.Sku);
            }

            var p = LegacyProductCatalog.Products[line.Sku];
            subtotal = subtotal + p.Price * line.Quantity;
            weight = weight + p.WeightKg * line.Quantity;
        }

        // --- скидка ---
        decimal rate = 0;
        if (request.Tier == CustomerTier.Silver)
        {
            rate = 0.05m;
        }
        else if (request.Tier == CustomerTier.Gold)
        {
            rate = 0.10m;
        }

        if (subtotal >= 20000)
        {
            rate = rate + 0.03m;
        }

        decimal discount = Math.Round(subtotal * rate, 2, MidpointRounding.AwayFromZero);
        decimal afterDiscount = subtotal - discount;

        // --- доставка ---
        decimal delivery;
        string deliveryName;
        string type = (request.DeliveryType ?? "").Trim().ToLowerInvariant();
        switch (type)
        {
            case "pickup":
                delivery = 0;
                deliveryName = "самовывоз";
                break;
            case "courier":
                if (afterDiscount >= 5000)
                {
                    delivery = 0;
                }
                else
                {
                    delivery = 350;
                }

                deliveryName = "курьер";
                break;
            case "post":
                decimal extraKg = Math.Ceiling(Math.Max(0m, weight - 1m));
                delivery = 250 + 60 * extraKg;
                deliveryName = "почта";
                break;
            case "express":
                if (request.OrderDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    throw new InvalidOperationException("Экспресс-доставка не работает по воскресеньям");
                }

                delivery = 900;
                if (weight > 5)
                {
                    delivery = delivery + 500;
                }

                deliveryName = "экспресс";
                break;
            default:
                throw new ArgumentException("Неизвестный способ доставки: " + request.DeliveryType);
        }

        decimal total = afterDiscount + delivery;

        // --- номер заказа ---
        _counter++;
        string number = "ORD-" + request.OrderDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
            + "-" + _counter.ToString("D4", CultureInfo.InvariantCulture);

        // --- уведомление ---
        string message = "Заказ " + number + " оформлен. Сумма: " + total.ToString("0.00", CultureInfo.InvariantCulture)
            + " руб. Доставка: " + deliveryName;
        var smtp = new SmtpEmailClient();
        smtp.Send(request.CustomerEmail, message);

        // --- сохранение ---
        var result = new OrderResult(number, subtotal, discount, delivery, total, type, message);
        _orders[number] = result;

        Console.WriteLine("[LOG] Заказ " + number + " сохранён");
        return result;
    }

    public OrderResult? GetOrder(string orderNumber)
    {
        if (_orders.ContainsKey(orderNumber))
        {
            return _orders[orderNumber];
        }

        return null;
    }
}

/// <summary>Legacy: статический «каталог» — глобальное состояние, которое нельзя подменить в тестах.</summary>
internal static class LegacyProductCatalog
{
    public static readonly Dictionary<string, Product> Products = new Dictionary<string, Product>
    {
        ["BOOK-001"] = new Product("BOOK-001", "Книга «Основы C#»", 1500.00m, 0.6m),
        ["MUG-001"] = new Product("MUG-001", "Кружка разработчика", 500.00m, 0.4m),
        ["KB-001"] = new Product("KB-001", "Механическая клавиатура", 6500.00m, 1.2m),
        ["MON-001"] = new Product("MON-001", "Монитор 27\"", 18990.00m, 6.5m),
        ["CABLE-01"] = new Product("CABLE-01", "Кабель USB-C", 390.00m, 0.1m),
        ["STICKER-01"] = new Product("STICKER-01", "Набор стикеров", 199.90m, 0.05m),
    };
}

/// <summary>Legacy: «почтовый клиент», создаётся через new прямо внутри Process.</summary>
internal sealed class SmtpEmailClient
{
    public void Send(string to, string body)
    {
        Console.WriteLine("[SMTP] to=" + to + ": " + body);
    }
}

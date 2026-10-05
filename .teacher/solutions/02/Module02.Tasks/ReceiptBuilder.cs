using System.Globalization;
using System.Text;

namespace Module02.Tasks;

public static class ReceiptBuilder
{
    private const string MoneyFormat = "0.00";

    public static string BuildReceipt(int number, DateOnly date, ReceiptLine[] lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        // StringBuilder, а не string += в цикле: строки immutable, и каждая конкатенация
        // копирует всю накопленную строку заново — O(n²) по памяти и времени на 1000 строк чека.
        // StringBuilder дописывает во внутренний буфер и создаёт итоговую строку один раз.
        var sb = new StringBuilder();

        // Культура указана ЯВНО везде: чек — машиночитаемый текст, на русской Windows
        // без InvariantCulture получили бы "5,00".
        sb.Append("Receipt #")
          .Append(number.ToString(CultureInfo.InvariantCulture))
          .Append(" (")
          .Append(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
          .Append(")\n"); // '\n' вместо AppendLine: AppendLine добавляет Environment.NewLine ("\r\n" на Windows)

        var total = 0m;
        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
            {
                throw new ArgumentException($"Количество должно быть положительным: '{line.Name}'.", nameof(lines));
            }

            if (line.Price < 0)
            {
                throw new ArgumentException($"Цена не может быть отрицательной: '{line.Name}'.", nameof(lines));
            }

            var lineTotal = line.Price * line.Quantity; // decimal * int → decimal
            total += lineTotal;

            // Append возвращает тот же StringBuilder — поэтому вызовы можно чейнить (fluent interface).
            sb.Append(line.Name)
              .Append(" x")
              .Append(line.Quantity.ToString(CultureInfo.InvariantCulture))
              .Append(" = ")
              .Append(lineTotal.ToString(MoneyFormat, CultureInfo.InvariantCulture))
              .Append('\n');
        }

        sb.Append("Total: ")
          .Append(total.ToString(MoneyFormat, CultureInfo.InvariantCulture))
          .Append('\n');

        return sb.ToString();
    }
}

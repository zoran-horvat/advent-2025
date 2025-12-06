using System.Text;

static class Day06
{
    public static void Run(TextReader reader)
    {
        var input = reader.ReadRawInput();

        ulong horizontalSum = input.ParseHorizontally().Aggregate();
        ulong verticalSum = input.ParseVertically().Aggregate();

        Console.WriteLine($"Sum of all horizontal calculations: {horizontalSum}");
        Console.WriteLine($"Sum of all vertical calculations:   {verticalSum}");
    }

    private static ulong Aggregate(this IEnumerable<string> fields)
    {
        ulong sum = 0UL;
        List<ulong> pending = new();

        foreach (var field in fields)
        {
            if (field == "*")
            {
                sum += pending.Aggregate(1UL, (acc, val) => acc * val);
                pending.Clear();
            }
            else if (field == "+")
            {
                sum += pending.Aggregate(0UL, (acc, val) => acc + val);
                pending.Clear();
            }
            else if (ulong.TryParse(field, out var number))
            {
                pending.Add(number);
            }
            else
            {
                throw new InvalidDataException($"Invalid field: {field}");
            }
        }

        return sum;
    }

    private static IEnumerable<string> ParseVertically(this string[] rawInput)
    {
        for (int col = rawInput.Max(line => line.Length) - 1; col >= 0; col--)
        {
            int row = 0;
            while (row < rawInput.Length)
            {
                while (row < rawInput.Length && (col >= rawInput[row].Length || rawInput[row][col] == ' ')) row++;

                var segment = new StringBuilder();
                while (row < rawInput.Length && col < rawInput[row].Length && rawInput[row][col] != ' ')
                {
                    if ("*+".Contains(rawInput[row][col]) && segment.Length > 0)
                    {
                        yield return segment.ToString();
                        segment.Clear();
                    }
                    segment.Append(rawInput[row++][col]);
                }

                if (segment.Length > 0) yield return segment.ToString();
            }
        }
    }

    private static IEnumerable<string> ParseHorizontally(this string[] rawInput)
    {
        var parts = rawInput.Select(line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToArray();
        int rowsCount = parts.Length;
        int columnsCount = parts[0].Length;

        for (int col = 0; col < columnsCount; col++)
        {
            for (int row = 0; row < rowsCount; row++)
            {
                yield return parts[row][col];
            }
        }
    }

    private static string[] ReadRawInput(this TextReader reader) =>
        reader.ReadLines().ToArray();
}
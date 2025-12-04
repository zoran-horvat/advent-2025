static class Day02
{
    public static void Run(TextReader reader)
    {
        var ranges = reader.ReadRanges().ToList();

        ulong sumInvalidHalf = ranges.Select(SumInvalidIdsHalfCut).Sum();

        Console.WriteLine($"Sum of all invalid IDs (half-split): {sumInvalidHalf}");
    }

    private static ulong Sum(this IEnumerable<ulong> splits) =>
        splits.Aggregate(0UL, (acc, val) => acc + val);

    private static ulong SumInvalidIdsHalfCut(this Range range)
    {
        if (range.To < range.From) return 0;
        if (range.DigitsCount % 2 != 0) return 0;

        ulong divisor = (range.DigitsCount / 2).GetDivisor();
        var segments = range.IsolateSegments(divisor);

        ulong sum = 0;

        foreach (var segment in segments)
        {
            ulong from = Math.Max(segment.From / divisor, segment.From % divisor);
            ulong to = Math.Min(segment.To / divisor, segment.To % divisor);

            if (to < from) continue;

            ulong halfSum = (to * (to + 1) - (from - 1) * from)  / 2;
            sum += halfSum * divisor + halfSum;
        }

        return sum;
    }

    private static IEnumerable<NumbersRange> IsolateSegments(this Range range, ulong divisor)
    {
        ulong fromUp = range.From / divisor;
        ulong toUp = range.To / divisor;

        if (fromUp == toUp)
        {
            yield return new NumbersRange(range.From, range.To);
        }
        else if (fromUp == toUp - 1)
        {
            yield return new NumbersRange(range.From, range.From / divisor * divisor + divisor - 1);
            yield return new NumbersRange((range.From / divisor + 1) * divisor, range.To);
        }
        else
        {
            yield return new NumbersRange(range.From, (range.From / divisor + 1) * divisor - 1);
            yield return new NumbersRange((range.From / divisor + 1) * divisor, range.To / divisor * divisor - 1);
            yield return new NumbersRange(range.To / divisor * divisor, range.To);
        }
    }

    private static ulong GetDivisor(this int digitsCount) =>
        (ulong)Math.Pow(10, digitsCount);

    private static IEnumerable<Range> ReadRanges(this TextReader reader) => 
        reader.ReadLines()
            .SelectMany(line => line.Split(','))
            .Where(pair => !string.IsNullOrWhiteSpace(pair))
            .Select(pair => pair.Split('-'))
            .Select(pair => (from: ulong.Parse(pair[0]), to: ulong.Parse(pair[1])))
            .SelectMany(ToRanges);

    private static IEnumerable<Range> ToRanges(this (ulong from, ulong to) bounds)
    {
        int fromDigits = bounds.from.CountDigits();
        int toDigits = bounds.to.CountDigits();
        for (int digits = fromDigits; digits <= toDigits; digits++)
        {
            ulong newTo = Math.Min(bounds.to, (ulong)Math.Pow(10, digits) - 1);
            yield return new Range(bounds.from, newTo, digits);
            bounds.from = newTo + 1;
            if (bounds.from > bounds.to) yield break;
        }
    }

    private static int CountDigits(this ulong number) =>
        number == 0 ? 1 : (int)Math.Floor(Math.Log10(number)) + 1;

    record NumbersRange(ulong From, ulong To);
    record Split(ulong Number, int PartsCount);
    record Range(ulong From, ulong To, int DigitsCount);
}
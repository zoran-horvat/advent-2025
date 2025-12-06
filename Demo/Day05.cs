static class Day05
{
    public static void Run(TextReader reader)
    {
        var ranges = reader.ReadRanges().Compact().ToList();
        var items = reader.ReadItems().ToList();

        int freshCount = items.Count(item => ranges.Any(range => range.Contains(item)));
        ulong totalSize = ranges.SumSizes();

        Console.WriteLine($"Total count of IDs in all ranges: {freshCount}");
        Console.WriteLine($"Total size of all ranges:         {totalSize}");
    }
    
    private static ulong SumSizes(this IEnumerable<Range> ranges) =>
        ranges.Aggregate(0UL, (acc, range) => acc + range.GetSize());

    private static IEnumerable<Range> Compact(this IEnumerable<Range> ranges)
    {
        var boundaries = ranges
            .SelectMany(range => new[] { (value: range.From, change: +1), (value: range.To + 1, change: -1)})
            .GroupBy(b => b.value, (value, group) => (value, change: group.Sum(b => b.change)))
            .OrderBy(b => b.value);
        
        int insideCount = 0;
        ulong rangeStart = 0;

        foreach (var (value, change) in boundaries)
        {
            int newInsideCount = insideCount + change;
            if (insideCount == 0 && newInsideCount > 0) rangeStart = value;
            if (insideCount > 0 && newInsideCount == 0) yield return new Range(rangeStart, value - 1);
            insideCount = newInsideCount;
        }
    }

    private static ulong GetSize(this Range range) =>
        range.To - range.From + 1;

    private static bool Contains(this Range range, ulong value) =>
        range.From <= value && value <= range.To;

    private static IEnumerable<ulong> ReadItems(this TextReader reader) =>
        reader.ReadLines().Select(ulong.Parse);

    private static IEnumerable<Range> ReadRanges(this TextReader reader) =>
        reader.ReadLines().TakeWhile(line => !string.IsNullOrWhiteSpace(line)).Select(ToRange);

    private static Range ToRange(this string line) =>
        line.Split('-').ToRange();

    private static Range ToRange(this string[] bounds) =>
        new Range(ulong.Parse(bounds[0]), ulong.Parse(bounds[1]));

    record Range(ulong From, ulong To);
}
using System.Drawing;

static class Day09
{
    public static void Run(TextReader reader)
    {
        var points = reader.ReadPoints().ToList();

        var maxArea = points.GetAllPairs().Select(GetArea).Max();

        Console.WriteLine($"Largest area between any two points: {maxArea}");
    }

    private static IEnumerable<(Point a, Point b)> GetAllPairs(this List<Point> points) =>
        from i in Enumerable.Range(0, points.Count - 1)
        from j in Enumerable.Range(i + 1, points.Count - i - 1)
        select (points[i], points[j]);

    private static long GetArea((Point a, Point b) pair) =>
        Math.Abs((long)(pair.a.Row - pair.b.Row + 1) * (pair.a.Column - pair.b.Column + 1));

    private static IEnumerable<Point> ReadPoints(this TextReader reader) =>
        reader.ReadLines()
            .Select(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(parts => new Point(int.Parse(parts[0]), int.Parse(parts[1])));

    record Point(int Row, int Column);
}
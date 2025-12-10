using System.Linq.Expressions;
using Microsoft.VisualBasic;

static class Day09
{
    public static void Run(TextReader reader)
    {
        var points = reader.ReadPoints().ToList();

        points.Draw(40);

        // 4582310446 too high
        // 2976014041 - not
        // 137489982 - not
    }

    private static void Draw(this IEnumerable<Point> points, int maxSize)
    {
        var set = points.ToHashSet();
        int minX = 0;
        int maxX = set.Max(p => p.X) + 2;
        int minY = 0;
        int maxY = set.Max(p => p.Y) + 2;

        if (maxX - minX > maxSize || maxY - minY > maxSize) return;

        if (maxX - minX > maxSize || maxY - minY > maxSize) return;
        
        var discriminators = points.ToList()
            .ToClockwiseLines()
            .GetHorizontalDiscriminators()
            .OrderByDescending(discriminator => discriminator switch 
            {
                EnterAt enterAt => enterAt.Line.Y,
                ExitBelow enterBelow => enterBelow.Line.Y - 1,
                _ => throw new ArgumentException("Unknown discriminator type.")
            })
            .ToList();

        discriminators.Select(ToLabel).ToList().ForEach(Console.WriteLine);

        bool IsInside(Point point) => discriminators.Where(d => d.Affects(point)).LastOrDefault() is EnterAt;

        Console.WriteLine($"({minX},{minY})");
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                var point = new Point(x, y);
                if (set.Contains(point) && IsInside(point)) Console.Write('O');
                else if (set.Contains(point)) Console.Write('#');
                else if (IsInside(point)) Console.Write('X');
                else Console.Write('.');
            }
            Console.WriteLine();
        }
    }

    private static string ToLabel(this Discriminator discriminator) => discriminator switch
    {
        EnterAt enterAt => $"EnterAt Y={enterAt.Line.Y} X=[{enterAt.Line.FromX}..{enterAt.Line.ToX}]",
        ExitBelow exitBelow => $"ExitBelow Y={exitBelow.Line.Y} X=[{exitBelow.Line.FromX}..{exitBelow.Line.ToX}]",
        _ => "Unknown discriminator"
    };

    private static IEnumerable<Discriminator> GetHorizontalDiscriminators(this IEnumerable<Line> segments)
    {
        var horizontal = segments.OfType<HorizontalLine>().ToList();
        var points = horizontal.Select(GetPoints).ToList();

        for (int i = 0; i < horizontal.Count; i++)
        {
            var prev = horizontal[(i + horizontal.Count - 1) % horizontal.Count];
            var current = horizontal[i];
            var next = horizontal[(i + 1) % horizontal.Count];
            var endpoints = current.GetPoints().ToArray();
            
            var discriminatorLine = (prev, current, next) switch
            {
                (HorizontalLine p, Right c, HorizontalLine n) when c.Y > p.Y && c.Y < n.Y => c.WithoutLastPoint(),
                (HorizontalLine p, Right c, HorizontalLine n) when c.Y > p.Y && c.Y > p.Y => c,
                (HorizontalLine p, Right c, HorizontalLine n) when c.Y < p.Y && c.Y < n.Y => c.WithoutFirstPoint().WithoutLastPoint(),
                (HorizontalLine p, Right c, HorizontalLine n) when c.Y < p.Y && c.Y > n.Y => c.WithoutFirstPoint(),
                (HorizontalLine p, Left c, HorizontalLine n) when c.Y > p.Y && c.Y < n.Y => c.WithoutFirstPoint(),
                (HorizontalLine p, Left c, HorizontalLine n) when c.Y > p.Y && c.Y > p.Y => c.WithoutFirstPoint().WithoutLastPoint(),
                (HorizontalLine p, Left c, HorizontalLine n) when c.Y < p.Y && c.Y < n.Y => c,
                (HorizontalLine p, Left c, HorizontalLine n) when c.Y < p.Y && c.Y > n.Y => c.WithoutLastPoint(),
                _ => throw new InvalidOperationException("Cannot determine discriminator type.")
            };

            yield return discriminatorLine.ToDiscriminator(endpoints);
        }
    }

    private static Discriminator ToDiscriminator(this Line line, Point[] points) => line switch
    {
        Right r => new EnterAt(r, points),
        Left l => new ExitBelow(l.Reverse(), points),
        _ => throw new ArgumentException("Only horizontal lines can be converted to discriminators.")
    };

    private static bool Affects(this Discriminator discriminator, Point point) => discriminator switch
    {
        EnterAt enterAt => point.Y <= enterAt.Line.Y && point.X >= enterAt.Line.FromX && point.X <= enterAt.Line.ToX,
        ExitBelow exitBelow => point.Y < exitBelow.Line.Y && point.X >= exitBelow.Line.FromX && point.X <= exitBelow.Line.ToX,
        _ => false
    };

    private static Right Reverse(this Left line) => new Right(line.Y, line.ToX, line.FromX);

    private static Line WithoutFirstPoint(this Line line) => line switch
    {
        Right r => new Right(r.Y, r.FromX + 1, r.ToX),
        Left l => new Left(l.Y, l.FromX - 1, l.ToX),
        Up u => new Up(u.X, u.FromY + 1, u.ToY),
        Down d => new Down(d.X, d.FromY - 1, d.ToY),
        _ => line
    };

    private static Line WithoutLastPoint(this Line line) => line switch
    {
        Right r => new Right(r.Y, r.FromX, r.ToX - 1),
        Left l => new Left(l.Y, l.FromX, l.ToX + 1),
        Up u => new Up(u.X, u.FromY, u.ToY - 1),
        Down d => new Down(d.X, d.FromY, d.ToY + 1),
        _ => line
    };

    private static Point[] GetPoints(this Line line) => line switch
    {
        Right r => [new Point(r.FromX, r.Y), new Point(r.ToX, r.Y)],
        Left l => [new Point(l.FromX, l.Y), new Point(l.ToX, l.Y)],
        Up u => [new Point(u.X, u.FromY), new Point(u.X, u.ToY)],
        Down d => [new Point(d.X, d.FromY), new Point(d.X, d.ToY)],
        _ => throw new ArgumentException("Unknown line type.")
    };

    private static IEnumerable<Line> ToClockwiseLines(this List<Point> points)
    {
        var segments = points.ToLines().ToList();
        var topMostSegments = segments.OfType<HorizontalLine>().MaxBy(line => line.Y);

        if (topMostSegments is Right) return segments;

        return ((IEnumerable<Point>)points).Reverse().ToList().ToLines();
    }

    private static IEnumerable<Line> ToLines(this List<Point> points) =>
        points.Zip(points[1..].Concat([points[0]]), (from, to) => from.LineTo(to));

    private static Line LineTo(this Point from, Point to) =>
        from.Y == to.Y && from.X < to.X ? new Right(from.Y, from.X, to.X)
        : from.Y == to.Y ? new Left(from.Y, from.X, to.X)
        : from.Y < to.Y ? new Up(from.X, from.Y, to.Y)
        : new Down(from.X, from.Y, to.Y);

    private static IEnumerable<Point> ReadPoints(this TextReader reader) =>
        reader.ReadLines()
            .Select(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(parts => new Point(int.Parse(parts[0]), int.Parse(parts[1])));

    record Index(Dictionary<int, List<ShapeSegment>> ShapesByX);
    record ShapeSegment(int MaxY, int Change);

    abstract record Discriminator;
    record EnterAt(Right Line, Point[] Points) : Discriminator;
    record ExitBelow(Right Line, Point[] Points) : Discriminator;
    
    abstract record Line;
    abstract record HorizontalLine(int Y) : Line;
    record Right(int Y, int FromX, int ToX) : HorizontalLine(Y);
    record Left(int Y, int FromX, int ToX) : HorizontalLine(Y);
    record Up(int X, int FromY, int ToY) : Line;
    record Down(int X, int FromY, int ToY) : Line;


    record Point(int X, int Y);
}
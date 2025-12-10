using System.Linq.Expressions;

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
        
        var shapes = points.ToList()
            .ToClockwiseSegments()
            .GetHorizontalDiscriminators()
            .SelectMany(ToShapeSegments);

        bool IsInside(Point point) =>
            shapes
                .Where(discriminator => discriminator.x == point.X)
                .Select(discriminator => discriminator.shape)
                .Where(shape => shape.MaxY >= point.Y)
                .OrderByDescending(shape => shape.MaxY)
                .LastOrDefault() is ShapeSegment shape && shape.Change > 0;

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

    private static IEnumerable<(int x, ShapeSegment shape)> ToShapeSegments(this Segment segment) =>
        Enumerable.Range(Math.Min(segment.To.X, segment.From.X), Math.Abs(segment.To.X - segment.From.X) + 1)
            .Select(x => (x, segment.ToShapeSegment()));

    private static ShapeSegment ToShapeSegment(this Segment segment) =>
        segment.To.X > segment.From.X ? new ShapeSegment(segment.From.Y, 1)
        : new ShapeSegment(segment.To.Y - 1, -1);

    private static IEnumerable<Segment> GetHorizontalDiscriminators(this IEnumerable<Segment> segments)
    {
        var horizontal = segments.Where(seg => seg.From.Y == seg.To.Y).ToList();

        for (int i = 0; i < horizontal.Count; i++)
        {
            var current = horizontal[i];
            var next = horizontal[(i + 1) % horizontal.Count];

            if (current.To.X != next.From.X) continue;

            var currentStep = Math.Sign(current.To.X - current.From.X);
            var nextStep = Math.Sign(next.To.X - next.From.X);

            var currentChange = 0;
            var nextChange = 0;

            var currentY = current.From.Y;
            var nextY = next.From.Y;

            if (currentStep > 0 && nextStep > 0 && currentY > nextY) nextChange = currentStep;
            else if (currentStep > 0 && nextStep > 0) currentChange = -currentStep;
            else if (currentStep > 0 && nextStep < 0 && currentY <= nextY) (currentChange, nextChange) = ( -currentStep, -currentStep);
            else if (currentStep < 0 && nextStep < 0 && currentY > next.To.Y) currentChange = -currentStep;
            else if (currentStep < 0 && nextStep < 0) nextChange = currentStep;
            else if (currentStep < 0 && nextStep > 0 && currentY > nextY) (currentChange, nextChange) = ( -currentStep, -currentStep);

            horizontal[i] = new Segment(current.From, new Point(current.To.X + currentChange, current.To.Y));
            horizontal[(i + 1) % horizontal.Count] = new Segment(new Point(next.From.X + nextChange, next.From.Y), next.To);
        }

        return horizontal;
    }

    private static IEnumerable<Segment> ToClockwiseSegments(this List<Point> points)
    {
        var segments = points.ToSegments().ToList();
        var topMostSegment = segments.Where(segment => segment.From.Y == segment.To.Y).MaxBy(segment => segment.From.Y);

        if (topMostSegment is null || topMostSegment.From.X < topMostSegment.To.X) return segments;

        return ((IEnumerable<Point>)points).Reverse().ToList().ToSegments();
    }

    private static IEnumerable<Segment> ToSegments(this List<Point> points) =>
        points.Zip(points[1..].Concat([points[0]]), (from, to) => new Segment(from, to));

    private static IEnumerable<Point> ReadPoints(this TextReader reader) =>
        reader.ReadLines()
            .Select(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(parts => new Point(int.Parse(parts[0]), int.Parse(parts[1])));

    record Index(Dictionary<int, List<ShapeSegment>> ShapesByX);
    record ShapeSegment(int MaxY, int Change);
    record Segment(Point From, Point To);
    record Point(int X, int Y);
}
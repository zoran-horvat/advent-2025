static class Day04
{
    public static void Run(TextReader reader)
    {
        var map = reader.ReadMap();

        var (immediateRemovable, totalRemovable) = map.CountRemovableRolls();

        Console.WriteLine($"Optimized immediate: {immediateRemovable}");
        Console.WriteLine($"Optimized total:     {totalRemovable}");
    }

    private static (int immediate, int total) CountRemovableRolls(this Map map)
    {
        var neighborsCount = map.Rolls.ToDictionary(roll => roll, roll => map.Neighbors[roll].Count(map.Rolls.Contains));
        var pending = new Queue<Position>(neighborsCount.Where(kv => kv.Value < 4).Select(kv => kv.Key));
        var remaining = map.Rolls.Except(pending).ToHashSet();

        var immediate = pending.Count;
        var total = immediate;

        while (pending.TryDequeue(out var roll))
        {
            foreach (var neighbor in map.Neighbors[roll].Where(remaining.Contains))
            {
                if (--neighborsCount[neighbor] < 4)
                {
                    pending.Enqueue(neighbor);
                    remaining.Remove(neighbor);
                    total++;
                }
            }
        }

        return (immediate, total);
    }
    
    private static Map ReadMap(this TextReader reader)
    {
        string[] rows = reader.ReadLines().ToArray();
        
        var rolls = new HashSet<Position>();
        var neighbors = new Dictionary<Position, List<Position>>();

        for (int row = 0; row < rows.Length; row++)
        {
            for (int column = 0; column < rows[row].Length; column++)
            {
                var position = new Position(row, column);
                string line = rows[row];

                if (rows[row][column] == '@') rolls.Add(position);

                List<Position> validNeighbors = new();
                if (position.Row > 0 && position.Column > 0) validNeighbors.Add(new Position(position.Row - 1, position.Column - 1));
                if (position.Row > 0) validNeighbors.Add(new Position(position.Row - 1, position.Column));
                if (position.Row > 0 && position.Column < line.Length - 1) validNeighbors.Add(new Position(position.Row - 1, position.Column + 1));
                if (position.Column > 0) validNeighbors.Add(new Position(position.Row, position.Column - 1));
                if (position.Column < line.Length - 1) validNeighbors.Add(new Position(position.Row, position.Column + 1));
                if (position.Row < rows.Length - 1 && position.Column > 0) validNeighbors.Add(new Position(position.Row + 1, position.Column - 1));
                if (position.Row < rows.Length - 1) validNeighbors.Add(new Position(position.Row + 1, position.Column));
                if (position.Row < rows.Length - 1 && position.Column < line.Length - 1) validNeighbors.Add(new Position(position.Row + 1, position.Column + 1));

                neighbors[position] = validNeighbors;
            }
        }

        return new(rolls, neighbors);
    }

    record struct Position(int Row, int Column);
    record Map(HashSet<Position> Rolls, Dictionary<Position, List<Position>> Neighbors);
}
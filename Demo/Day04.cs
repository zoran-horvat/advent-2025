static class Day04
{
    public static void Run(TextReader reader)
    {
        var map = reader.ReadMap();

        var accessibleRolls = map.Rolls.Select(pos => map.CountSurroundingRolls(pos)).Count(count => count < 4);
        var removableCount = map.CountTotalRemovableRolls();

        Console.WriteLine($"Number of accessible rolls: {accessibleRolls}");
        Console.WriteLine($"Number of removable rolls:  {removableCount}");
    }

    private static int CountTotalRemovableRolls(this Map map)
    {
        HashSet<Position> rolls = map.Rolls.ToHashSet();
        int originalRollsCount = rolls.Count;

        while (true)
        {
            var removable = rolls
                .Where(roll => map.Neighbors[roll].Where(rolls.Contains).Count() < 4)
                .ToList();

            if (removable.Count == 0) break;

            foreach (var roll in removable) rolls.Remove(roll);
        }

        return originalRollsCount - rolls.Count;
    }

    private static int CountSurroundingRolls(this Map map, Position position) =>
        map.Neighbors[position].Count(map.Rolls.Contains);
    
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
static class Day10
{
    public static void Run(TextReader reader)
    {
        var machines = reader.ReadMachines().ToList();

        var minButtonPressesIndicators = machines.Sum(SwitchIndicators);

        Console.WriteLine($"Minimum button presses: {minButtonPressesIndicators}");
    }

    private static int SwitchIndicators(this Machine machine)
    {
        var maxIndicators = new Indicators(machine.Buttons.Aggregate(0, (acc, button) => acc | button.Toggles));
        
        var infinite = int.MaxValue;
        var minCounts = Enumerable.Repeat(infinite, maxIndicators.Bits + 1).ToArray();
        minCounts[0] = 0;

        var reached = new HashSet<Indicators> { new Indicators(0) };

        foreach (var button in machine.Buttons)
        {
            var newReached = new HashSet<Indicators>(reached);
            foreach (var indicators in reached)
            {
                var newIndicators = indicators.Apply(button);
                if (minCounts[newIndicators.Bits] > minCounts[indicators.Bits] + 1)
                {
                    minCounts[newIndicators.Bits] = minCounts[indicators.Bits] + 1;
                    newReached.Add(newIndicators);
                }
            }
            reached = newReached;
        }

        if (minCounts[machine.Indicators.Bits] < infinite) return minCounts[machine.Indicators.Bits];

        throw new InvalidDataException("Cannot reach target indicators with available buttons.");
    }

    private static Indicators Apply(this Indicators indicators, Button button) =>
        new Indicators(indicators.Bits ^ button.Toggles);

    public record Machine(Indicators Indicators, Button[] Buttons, Joltages Joltages);

    public record Joltages(int[] Values);

    public record Button(int Toggles, int[] ToggleIndices);
    public record struct Indicators(int Bits);
}

file static class Day10Parsing
{
    public static IEnumerable<Day10.Machine> ReadMachines(this TextReader reader) =>
        reader.ReadLines().Select(ParseMachine);

    private static Day10.Machine ParseMachine(string line) =>
        new Day10.Machine(line.ParseIndicators(), line.ParseButtons().ToArray(), line.ParseJoltages());

    private static Day10.Joltages ParseJoltages(this string line) =>
        System.Text.RegularExpressions.Regex.Match(line, @"\{(?<numbers>\d+(,\d+)*)\}") is { Success: true } match &&
            match.Groups["numbers"] is { Success: true } group
            ? new Day10.Joltages(group.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray())
            : throw new InvalidDataException($"Invalid joltages line: {line}");

    private static IEnumerable<Day10.Button> ParseButtons(this string line) =>
        System.Text.RegularExpressions.Regex.Matches(line, @"\((?<numbers>\d+(,\d+)*)\)")
            .Where(match => match.Success && match.Groups["numbers"].Success)
            .Select(match => match.Groups["numbers"].Value)
            .Select(ParseButton);

    private static Day10.Button ParseButton(this string toggles) =>
        toggles.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray()
            .ToButton();

    private static Day10.Button ToButton(this int[]toggleIndices) =>
        new Day10.Button(toggleIndices.Aggregate(0, (acc, val) => acc | (1 << val)), toggleIndices);

    private static Day10.Indicators ParseIndicators(this string line) =>
        System.Text.RegularExpressions.Regex.Match(line, @"\[(?<indicators>[\.#]+)\]") is { Success: true } match &&
            match.Groups["indicators"] is { Success: true } group
            ? group.Value.ParseBits()
            : throw new InvalidDataException($"Invalid indicators line: {line}");

    private static Day10.Indicators ParseBits(this string remainingBits) =>
        new Day10.Indicators(remainingBits.Select((b, i) => b == '#' ? 1 << i : 0).Sum());    
}
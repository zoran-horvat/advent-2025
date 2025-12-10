using System.Text.RegularExpressions;

static class Day10
{
    public static void Run(TextReader reader)
    {
        var machines = reader.ReadMachines().ToList();

        var minButtonPresses = machines.Sum(GetMinimumButtonPresses);

        Console.WriteLine($"Minimum button presses: {minButtonPresses}");
    }

    private static int GetMinimumButtonPresses(this Machine machine)
    {
        var maxIndicators = new Indicators(machine.Buttons.Aggregate(0, (acc, button) => acc | button.Toggles));
        
        var minCounts = Enumerable.Repeat(int.MaxValue, maxIndicators.Bits + 1).ToArray();
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

        return minCounts[machine.Indicators.Bits];
    }

    private static Indicators Apply(this Indicators indicators, Button button) =>
        new Indicators(indicators.Bits ^ button.Toggles);

    private static IEnumerable<Machine> ReadMachines(this TextReader reader) =>
        reader.ReadLines().Select(line => new Machine(line.ParseIndicators(), line.ParseButtons().ToArray()));

    private static IEnumerable<Button> ParseButtons(this string line) =>
        Regex.Matches(line, @"\((?<numbers>\d+(,\d+)*)\)")
            .Where(match => match.Success && match.Groups["numbers"].Success)
            .Select(match => match.Groups["numbers"].Value)
            .Select(ParseButton);

    private static Button ParseButton(this string toggles) =>
        new Button(toggles.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .Aggregate(0, (acc, val) => acc | (1 << val)));

    private static Indicators ParseIndicators(this string line) =>
        Regex.Match(line, @"\[(?<indicators>[\.#]+)\]") is { Success: true } match &&
            match.Groups["indicators"] is { Success: true } group
            ? group.Value.ParseBits()
            : throw new InvalidDataException($"Invalid indicators line: {line}");

    private static Indicators ParseBits(this string remainingBits) =>
        new Indicators(remainingBits.Select((b, i) => b == '#' ? 1 << i : 0).Sum());

    record Machine(Indicators Indicators, Button[] Buttons);

    record struct Button(int Toggles);
    record struct Indicators(int Bits);
}
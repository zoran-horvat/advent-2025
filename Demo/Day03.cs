static class Day03
{
    public static void Run(TextReader reader)
    {
        var batteryRanks = reader.ReadBatteryBanks().ToList();

        var totalJoltage2 = batteryRanks.Sum(GetTwoBatteryRating);
        var totalJoltage12 = batteryRanks.Sum(GetTwelveBatteryRating);

        Console.WriteLine($"Total joltage (2 batteries):  {totalJoltage2}");
        Console.WriteLine($"Total joltage (12 batteries): {totalJoltage12}");
    }

    private static long GetTwoBatteryRating(this BatteryBank bank) =>
        long.Parse(bank.GetBatteryRating(string.Empty, 2));
    
    private static long GetTwelveBatteryRating(this BatteryBank bank) =>
        long.Parse(bank.GetBatteryRating(string.Empty, 12));

    private static string GetBatteryRating(this BatteryBank bank, string turnedOn, int remainingSlots)
    {
        if (remainingSlots == 0) return turnedOn;

        int indexToTurnOn = bank.SelectBatteryIndexToTurnOn(remainingSlots - 1);
        turnedOn += bank.Ratings[indexToTurnOn];
        bank = new BatteryBank(bank.Ratings[(indexToTurnOn + 1)..]);

        return bank.GetBatteryRating(turnedOn, remainingSlots - 1);
    }   

    private static int SelectBatteryIndexToTurnOn(this BatteryBank bank, int remainingSlots) =>
        bank.Ratings.IndexOf(bank.SelectBatteryJoltageToTurnOn(remainingSlots));

    private static char SelectBatteryJoltageToTurnOn(this BatteryBank bank, int remainingSlots) =>
        bank.Ratings[..^remainingSlots].ToCharArray().Max();

    private static IEnumerable<BatteryBank> ReadBatteryBanks(this TextReader reader) =>
        reader.ReadLines().Select(line => new BatteryBank(line));

    record BatteryBank(string Ratings);
}
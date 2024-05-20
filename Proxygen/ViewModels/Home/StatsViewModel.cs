using SharedModel.Model;

public class StatsViewModel
{
    public required UpdateStatus[] LastSevenUpdates { get; init; }
    public required SearchRecord[] LastSevenDays { get; init; }
    public required SearchRecord[] LastTwentyFourHours { get; init; }
}

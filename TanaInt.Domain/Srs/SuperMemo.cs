namespace TanaInt.Domain.Srs;

public class SrsItem
{
    public int Interval { get; set; }
    public int Repetition { get; set; }
    public double EFactor { get; set; }
}

public enum SuperMemoGrade
{
    Zero = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5
}
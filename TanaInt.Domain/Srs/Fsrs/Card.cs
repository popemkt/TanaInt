using System.Globalization;
using System.Text.RegularExpressions;

namespace TanaInt.Domain.Srs.Fsrs;

public class Card
{
    public DateTime Due { get; set; }
    public double Stability { get; set; }
    public double Difficulty { get; set; }
    public int ElapsedDays { get; set; }
    public int ScheduledDays { get; set; }
    public int Reps { get; set; }
    public int Lapses { get; set; }
    public State State { get; set; }
    public DateTime LastReview { get; set; }

    public Card(DateTime now)
    {
        Due = now;
        Stability = 0d;
        Difficulty = 0d;
        ElapsedDays = 0;
        ScheduledDays = 0;
        Reps = 0;
        Lapses = 0;
        State = State.New;
        LastReview = now;
    }
    
    public Card(
        DateTime due, 
        double stability, 
        double difficulty, 
        int elapsedDays, 
        int scheduledDays, 
        int reps, 
        int lapses, 
        State state, 
        DateTime lastReview)
    {
        Due = due;
        Stability = stability;
        Difficulty = difficulty;
        ElapsedDays = elapsedDays;
        ScheduledDays = scheduledDays;
        Reps = reps;
        Lapses = lapses;
        State = state;
        LastReview = lastReview;
    }

    public Card(Card card)
    {
        Due = card.Due;
        Stability = card.Stability;
        Difficulty = card.Difficulty;
        ElapsedDays = card.ElapsedDays;
        ScheduledDays = card.ScheduledDays;
        Reps = card.Reps;
        Lapses = card.Lapses;
        State = card.State;
        LastReview = card.LastReview;
    }

    public static Card FromTanaString(string tanaString, bool parseLastReview = true)
    {
        if (string.IsNullOrWhiteSpace(tanaString))
            throw new ArgumentException($"{nameof(tanaString)} is required");
        
        var regex = new Regex(@"\[\[date:(.+?)\]\](.+)", RegexOptions.Compiled);
        var match = regex.Match(tanaString);
        if (!match.Success)
            throw new ArgumentException("Invalid TanaString");
        
        var cardDue = DateTime.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
            // Kind needs to be unspecified since all date times belong to a timezone, this is to ease comparisons
            .WithKind(DateTimeKind.Unspecified);

        var srsValues = match.Groups[2].Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var cardStability = double.Parse(srsValues[0]);
        var cardDifficulty = double.Parse(srsValues[1]);
        var cardElapsedDays = int.Parse(srsValues[2]);
        var cardScheduledDays = int.Parse(srsValues[3]);
        var cardReps = int.Parse(srsValues[4]);
        var cardLapses = int.Parse(srsValues[5]);
        var cardState =  Enum.Parse<State>(srsValues[6]);
        var cardLastReview = 
            parseLastReview
                ? DateTime.Parse(srsValues[7], CultureInfo.InvariantCulture)
                    .WithKind(DateTimeKind.Unspecified)
                : DateTime.Now;

        return new Card(
            due: cardDue,
            stability: cardStability,
            difficulty: cardDifficulty,
            elapsedDays: cardElapsedDays,
            scheduledDays: cardScheduledDays,
            reps: cardReps,
            lapses: cardLapses,
            state: cardState,
            lastReview: cardLastReview
        );
    }

    public string ToTanaString() =>
        $"{Utils.FormatTanaDateTime(Due)}/{Stability}/{Difficulty}/{ElapsedDays}/{ScheduledDays}/{Reps}/{Lapses}/{State}/{LastReview:O}";
}
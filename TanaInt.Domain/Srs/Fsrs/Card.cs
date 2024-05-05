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

    //TODO there should be
    //Card.CreateNew with required now
    //Card contructor with required properties
    //Card(card)
    public Card(DateTime? now = null)
    {
        //TODO fix all the Now usages
        Due = now ?? DateTime.Now;
        Stability = 0;
        Difficulty = 0;
        ElapsedDays = 0;
        ScheduledDays = 0;
        Reps = 0;
        Lapses = 0;
        State = State.New;
        LastReview = now ?? DateTime.Now;
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

    public static Card FromTanaString(string? tanaString, bool parseLastReview = true)
    {
        if (string.IsNullOrWhiteSpace(tanaString)) return new Card();
        var regex = new Regex(@"\[\[date:(.+?)\]\](.+)");
        var match = regex.Match(tanaString);
        if (match.Success)
        {
            var card = new Card();
            var due = DateTime.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)
                // Kind needs to be unspecified since all date times belong to a timezone, this is to ease comparisons
                .WithKind(DateTimeKind.Unspecified);
            card.Due = due;

            var srsValues = match.Groups[2].Value.Split('/', StringSplitOptions.RemoveEmptyEntries);
            card.Stability = double.Parse(srsValues[0]);
            card.Difficulty = double.Parse(srsValues[1]);
            card.ElapsedDays = int.Parse(srsValues[2]);
            card.ScheduledDays = int.Parse(srsValues[3]);
            card.Reps = int.Parse(srsValues[4]);
            card.Lapses = int.Parse(srsValues[5]);
            card.State = Enum.Parse<State>(srsValues[6]);
            card.LastReview =
                parseLastReview
                    ? DateTime.Parse(srsValues[7], CultureInfo.InvariantCulture)
                        .WithKind(DateTimeKind.Unspecified)
                    : DateTime.Now;

            return card;
        }

        throw new ArgumentException("Invalid TanaString");
    }

    public string ToTanaString() =>
        $"{Utils.FormatTanaDateTime(Due)}/{Stability}/{Difficulty}/{ElapsedDays}/{ScheduledDays}/{Reps}/{Lapses}/{State}/{LastReview:O}";
}
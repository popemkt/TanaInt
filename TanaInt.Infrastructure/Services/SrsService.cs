using TanaInt.Domain.Srs;

namespace TanaInt.Infrastructure.Services;

public class SrsService
{
    public static SrsItem GenerateNext(SrsItem item, SuperMemoGrade grade)
    {
        int nextInterval;
        int nextRepetition;
        double nextEfactor;

        if (grade >= SuperMemoGrade.Three)
        {
            if (item.Repetition == 0)
            {
                nextInterval = 1;
                nextRepetition = 1;
            }
            else if (item.Repetition == 1)
            {
                nextInterval = 6;
                nextRepetition = 2;
            }
            else
            {
                nextInterval = (int)Math.Round(item.Interval * item.EFactor);
                nextRepetition = item.Repetition + 1;
            }
        }
        else
        {
            nextInterval = 1;
            nextRepetition = 0;
        }

        nextEfactor = item.EFactor + (0.1 - (5 - (int)grade) * (0.08 + (5 - (int)grade) * 0.02));

        if (nextEfactor < 1.3) nextEfactor = 1.3;

        return new SrsItem
        {
            Interval = nextInterval,
            Repetition = nextRepetition,
            EFactor = nextEfactor
        };
    }
}
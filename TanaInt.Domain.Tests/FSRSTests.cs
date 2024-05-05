using TanaInt.Domain.Srs.Fsrs;

namespace TanaInt.Domain.Tests;

using System;
using Xunit;

public class FSRSTests
{
    [Fact]
    public void Rating_Again_ShouldEqual1()
    {
        Assert.Equal(1, (int)Rating.Again);
    }

    [Fact]
    public void State_New_ShouldEqual0()
    {
        Assert.Equal(0, (int)State.New);
    }

    [Fact]
    public void Card_State_ShouldEqual0()
    {
        var card = new Card();
        Assert.Equal(0, (int)card.State);
    }

    [Fact]
    public void FSRS_P_W_ShouldMatchExpectedValues()
    {
        var fsrs = new FsrsAlgorithm(new Params());
        var expectedW = new double[]
        {
            0.4, 0.6, 2.4, 5.8, 4.93, 0.94, 0.86, 0.01, 1.49, 0.14, 0.94, 2.18, 0.05,
            0.34, 1.26, 0.29, 2.61
        };
        Assert.Equal(expectedW, fsrs.P.W);
    }

    [Fact]
    public void TestRepeat()
    {
        var f = new FsrsAlgorithm(new Params(maximumInterval: 36500));
        var cardTest = new Card();
        f.P.W = new double[]
        {
            1.14, 1.01, 5.44, 14.67, 5.3024, 1.5662, 1.2503, 0.0028, 1.5489, 0.1763,
            0.9953, 2.7473, 0.0179, 0.3105, 0.3976, 0.0, 2.0902
        };

        var now = new DateTime(2022, 11, 29, 12, 30, 0, 0, DateTimeKind.Utc);
        var schedulingCards = f.Repeat(cardTest, now);

        var ratings = new Rating[]
        {
            Rating.Good,
            Rating.Good,
            Rating.Good,
            Rating.Good,
            Rating.Good,
            Rating.Good,
            Rating.Again,
            Rating.Again,
            Rating.Good,
            Rating.Good,
            Rating.Good,
            Rating.Good,
            Rating.Good
        };
        var ivlHistory = new int[ratings.Length];
        var stateHistory = new State[ratings.Length];

        for (int i = 0; i < ratings.Length; i++)
        {
            cardTest = schedulingCards[ratings[i]].Card;
            //This lines for testing the conversion from TanaString to Card and back
            cardTest = Card.FromTanaString(cardTest.ToTanaString(), true);
            ivlHistory[i] = cardTest.ScheduledDays;
            stateHistory[i] = schedulingCards[ratings[i]].ReviewLog.State;
            now = cardTest.Due;
            schedulingCards = f.Repeat(cardTest, now);
        }

        var expectedIvlHistory = new int[]
        {
            0, 5, 16, 43, 106, 236, 0, 0, 12, 25, 47, 85, 147
        };
        var expectedStateHistory = new State[]
        {
            State.New,
            State.Learning,
            State.Review,
            State.Review,
            State.Review,
            State.Review,
            State.Review,
            State.Relearning,
            State.Relearning,
            State.Review,
            State.Review,
            State.Review,
            State.Review
        };

        Assert.Equal(expectedIvlHistory, ivlHistory);
        Assert.Equal(expectedStateHistory, stateHistory);
    }

    [Fact]
    public void ReviewLog_ElapsedDays_ShouldBeZero_ForNewCards_WhenScheduledDaysIsSet()
    {
        var fsrs = new FsrsAlgorithm(new Params());
        var card = new Card();
        card.ScheduledDays = 42;

        var log = fsrs.Repeat(card, new DateTime(2023, 11, 10, 23, 20, 47, 297, DateTimeKind.Utc));
        var againReviewLog = log[Rating.Again].ReviewLog;

        Assert.Equal(0, againReviewLog.ElapsedDays);
    }

    [Fact]
    public void ReviewLog_ScheduledDays_ShouldBeZero_ForAgainRatings_IrregardlessOfElapsedDaysSinceLastReview()
    {
        var fsrs = new FsrsAlgorithm(new Params());
        var card = new Card();
        card.State = State.Learning;
        card.LastReview = new DateTime(2023, 11, 6, 23, 20, 47, 297, DateTimeKind.Utc);

        var log = fsrs.Repeat(card, new DateTime(2023, 11, 10, 23, 20, 47, 297, DateTimeKind.Utc));
        var againReviewLog = log[Rating.Again].ReviewLog;

        Assert.Equal(0, againReviewLog.ScheduledDays);
    }
}
// Credits goes to https://github.com/open-spaced-repetition/fsrs.js
namespace TanaInt.Domain.Srs.Fsrs;

using System;
using System.Collections.Generic;

public enum State
{
    New = 0,
    Learning = 1,
    Review = 2,
    Relearning = 3,
}

public enum Rating
{
    Again = 1,
    Hard = 2,
    Good = 3,
    Easy = 4,
}

public class ReviewLog
{
    public Rating Rating { get; set; }
    public int ScheduledDays { get; set; }
    public int ElapsedDays { get; set; }
    public DateTime Review { get; set; }
    public State State { get; set; }

    public ReviewLog(Rating rating, int scheduledDays, int elapsedDays, DateTime review, State state)
    {
        Rating = rating;
        ElapsedDays = elapsedDays;
        ScheduledDays = scheduledDays;
        Review = review;
        State = state;
    }
}

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

    public Card()
    {
        Due = DateTime.Now;
        Stability = 0;
        Difficulty = 0;
        ElapsedDays = 0;
        ScheduledDays = 0;
        Reps = 0;
        Lapses = 0;
        State = State.New;
        LastReview = DateTime.Now;
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
}

public class SchedulingInfo
{
    public Card Card { get; set; }
    public ReviewLog ReviewLog { get; set; }

    public SchedulingInfo(Card card, ReviewLog reviewLog)
    {
        Card = card;
        ReviewLog = reviewLog;
    }
}

public class SchedulingCards
{
    public Card Again { get; set; }
    public Card Hard { get; set; }
    public Card Good { get; set; }
    public Card Easy { get; set; }

    public SchedulingCards(Card card)
    {
        Again = new Card(card);
        Hard = new Card(card);
        Good = new Card(card);
        Easy = new Card(card);
    }

    public void UpdateState(State state)
    {
        if (state == State.New)
        {
            Again.State = State.Learning;
            Hard.State = State.Learning;
            Good.State = State.Learning;
            Easy.State = State.Review;
        }
        else if (state == State.Learning || state == State.Relearning)
        {
            Again.State = state;
            Hard.State = state;
            Good.State = State.Review;
            Easy.State = State.Review;
        }
        else if (state == State.Review)
        {
            Again.State = State.Relearning;
            Hard.State = State.Review;
            Good.State = State.Review;
            Easy.State = State.Review;
            Again.Lapses += 1;
        }
    }

    public void Schedule(DateTime now, int hardInterval, int goodInterval, int easyInterval)
    {
        Again.ScheduledDays = 0;
        Hard.ScheduledDays = hardInterval;
        Good.ScheduledDays = goodInterval;
        Easy.ScheduledDays = easyInterval;

        Again.Due = now.AddMinutes(5);
        if (hardInterval > 0)
        {
            Hard.Due = now.AddDays(hardInterval);
        }
        else
        {
            Hard.Due = now.AddMinutes(10);
        }
        Good.Due = now.AddDays(goodInterval);
        Easy.Due = now.AddDays(easyInterval);
    }

    public Dictionary<Rating, SchedulingInfo> RecordLog(Card card, DateTime now)
    {
        return new Dictionary<Rating, SchedulingInfo>
        {
            [Rating.Again] = new SchedulingInfo(Again, new ReviewLog(Rating.Again, Again.ScheduledDays, card.ElapsedDays, now, card.State)),
            [Rating.Hard] = new SchedulingInfo(Hard, new ReviewLog(Rating.Hard, Hard.ScheduledDays, card.ElapsedDays, now, card.State)),
            [Rating.Good] = new SchedulingInfo(Good, new ReviewLog(Rating.Good, Good.ScheduledDays, card.ElapsedDays, now, card.State)),
            [Rating.Easy] = new SchedulingInfo(Easy, new ReviewLog(Rating.Easy, Easy.ScheduledDays, card.ElapsedDays, now, card.State)),
        };
    }
}

public class Params
{
    public double RequestRetention { get; set; }
    public int MaximumInterval { get; set; }
    public double[] W { get; set; }

    public Params()
    {
        RequestRetention = 0.9;
        MaximumInterval = 36500;
        W = new double[] { 0.4, 0.6, 2.4, 5.8, 4.93, 0.94, 0.86, 0.01, 1.49, 0.14, 0.94, 2.18, 0.05, 0.34, 1.26, 0.29, 2.61 };
    }
}

public class FSRS
{
    public Params P { get; set; }

    public FSRS()
    {
        P = new Params();
    }

    public Dictionary<Rating, SchedulingInfo> Repeat(Card card, DateTime now)
    {
        var clonedCard = new Card(card);
        if (clonedCard.State == State.New)
        {
            clonedCard.ElapsedDays = 0;
        }
        else
        {
            clonedCard.ElapsedDays = (int)(now - clonedCard.LastReview).TotalDays;
        }
        clonedCard.LastReview = now;
        clonedCard.Reps += 1;
        SchedulingCards s = new SchedulingCards(clonedCard);
        s.UpdateState(clonedCard.State);
        if (clonedCard.State == State.New)
        {
            InitDs(s);
            s.Again.Due = now.AddMinutes(1);
            s.Hard.Due = now.AddMinutes(5);
            s.Good.Due = now.AddMinutes(10);
            int easyInterval = NextInterval(s.Easy.Stability);
            s.Easy.ScheduledDays = easyInterval;
            s.Easy.Due = now.AddDays(easyInterval);
        }
        else if (clonedCard.State == State.Learning || clonedCard.State == State.Relearning)
        {
            int hardInterval = 0;
            int goodInterval = NextInterval(s.Good.Stability);
            int easyInterval = Math.Max(NextInterval(s.Easy.Stability), goodInterval + 1);
            s.Schedule(now, hardInterval, goodInterval, easyInterval);
        }
        else if (clonedCard.State == State.Review)
        {
            int interval = clonedCard.ElapsedDays;
            double lastD = clonedCard.Difficulty;
            double lastS = clonedCard.Stability;
            double retrievability = Math.Pow(1 + interval / (9 * lastS), -1);
            NextDs(s, lastD, lastS, retrievability);
            int hardInterval = NextInterval(s.Hard.Stability);
            int goodInterval = NextInterval(s.Good.Stability);
            hardInterval = Math.Min(hardInterval, goodInterval);
            goodInterval = Math.Max(goodInterval, hardInterval + 1);
            int easyInterval = Math.Max(NextInterval(s.Easy.Stability), goodInterval + 1);
            s.Schedule(now, hardInterval, goodInterval, easyInterval);
        }
        return s.RecordLog(clonedCard, now);
    }

    private void InitDs(SchedulingCards s)
    {
        s.Again.Difficulty = InitDifficulty(Rating.Again);
        s.Again.Stability = InitStability(Rating.Again);
        s.Hard.Difficulty = InitDifficulty(Rating.Hard);
        s.Hard.Stability = InitStability(Rating.Hard);
        s.Good.Difficulty = InitDifficulty(Rating.Good);
        s.Good.Stability = InitStability(Rating.Good);
        s.Easy.Difficulty = InitDifficulty(Rating.Easy);
        s.Easy.Stability = InitStability(Rating.Easy);
    }

    private void NextDs(SchedulingCards s, double lastD, double lastS, double retrievability)
    {
        s.Again.Difficulty = NextDifficulty(lastD, Rating.Again);
        s.Again.Stability = NextForgetStability(lastD, lastS, retrievability);
        s.Hard.Difficulty = NextDifficulty(lastD, Rating.Hard);
        s.Hard.Stability = NextRecallStability(lastD, lastS, retrievability, Rating.Hard);
        s.Good.Difficulty = NextDifficulty(lastD, Rating.Good);
        s.Good.Stability = NextRecallStability(lastD, lastS, retrievability, Rating.Good);
        s.Easy.Difficulty = NextDifficulty(lastD, Rating.Easy);
        s.Easy.Stability = NextRecallStability(lastD, lastS, retrievability, Rating.Easy);
    }

    private double InitStability(Rating r)
    {
        return Math.Max(P.W[(int)r - 1], 0.1);
    }

    private double InitDifficulty(Rating r)
    {
        return Math.Min(Math.Max(P.W[4] - P.W[5] * ((int)r - 3), 1), 10);
    }

    private int NextInterval(double s)
    {
        double interval = s * 9 * (1 / P.RequestRetention - 1);
        return Math.Min(Math.Max((int)Math.Round(interval), 1), P.MaximumInterval);
    }

    private double NextDifficulty(double d, Rating r)
    {
        double nextD = d - P.W[6] * ((int)r - 3);
        return Math.Min(Math.Max(MeanReversion(P.W[4], nextD), 1), 10);
    }

    private double MeanReversion(double init, double current)
    {
        return P.W[7] * init + (1 - P.W[7]) * current;
    }

    private double NextRecallStability(double d, double s, double r, Rating rating)
    {
        double hardPenalty = rating == Rating.Hard ? P.W[15] : 1;
        double easyBonus = rating == Rating.Easy ? P.W[16] : 1;
        return s * (1 + Math.Exp(P.W[8]) * (11 - d) * Math.Pow(s, -P.W[9]) * (Math.Exp((1 - r) * P.W[10]) - 1) * hardPenalty * easyBonus);
    }

    private double NextForgetStability(double d, double s, double r)
    {
        return P.W[11] * Math.Pow(d, -P.W[12]) * (Math.Pow(s + 1, P.W[13]) - 1) * Math.Exp((1 - r) * P.W[14]);
    }
}
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
    public Rating rating;
    public int scheduled_days;
    public int elapsed_days;
    public DateTime review;
    public State state;

    public ReviewLog(Rating rating, int scheduled_days, int elapsed_days, DateTime review, State state)
    {
        this.rating = rating;
        this.elapsed_days = elapsed_days;
        this.scheduled_days = scheduled_days;
        this.review = review;
        this.state = state;
    }
}

public class Card
{
    public DateTime due;
    public double stability;
    public double difficulty;
    public int elapsed_days;
    public int scheduled_days;
    public int reps;
    public int lapses;
    public State state;
    public DateTime last_review;

    public Card()
    {
        this.due = DateTime.Now;
        this.stability = 0;
        this.difficulty = 0;
        this.elapsed_days = 0;
        this.scheduled_days = 0;
        this.reps = 0;
        this.lapses = 0;
        this.state = State.New;
        this.last_review = DateTime.Now;
    }
}

public class SchedulingInfo
{
    public Card card;
    public ReviewLog review_log;

    public SchedulingInfo(Card card, ReviewLog review_log)
    {
        this.card = card;
        this.review_log = review_log;
    }
}

public class SchedulingCards
{
    public Card again;
    public Card hard;
    public Card good;
    public Card easy;

    public SchedulingCards(Card card)
    {
        this.again = new Card();
        this.hard = new Card();
        this.good = new Card();
        this.easy = new Card();
    }

    public void update_state(State state)
    {
        if (state == State.New)
        {
            this.again.state = State.Learning;
            this.hard.state = State.Learning;
            this.good.state = State.Learning;
            this.easy.state = State.Review;
        }
        else if (state == State.Learning || state == State.Relearning)
        {
            this.again.state = state;
            this.hard.state = state;
            this.good.state = State.Review;
            this.easy.state = State.Review;
        }
        else if (state == State.Review)
        {
            this.again.state = State.Relearning;
            this.hard.state = State.Review;
            this.good.state = State.Review;
            this.easy.state = State.Review;
            this.again.lapses += 1;
        }
    }

    public void schedule(DateTime now, int hard_interval, int good_interval, int easy_interval)
    {
        this.again.scheduled_days = 0;
        this.hard.scheduled_days = hard_interval;
        this.good.scheduled_days = good_interval;
        this.easy.scheduled_days = easy_interval;

        this.again.due = now.AddMinutes(5);
        if (hard_interval > 0)
        {
            this.hard.due = now.AddDays(hard_interval);
        }
        else
        {
            this.hard.due = now.AddMinutes(10);
        }
        this.good.due = now.AddDays(good_interval);
        this.easy.due = now.AddDays(easy_interval);
    }

    public Dictionary<Rating, SchedulingInfo> record_log(Card card, DateTime now)
    {
        return new Dictionary<Rating, SchedulingInfo>
        {
            [Rating.Again] = new SchedulingInfo(this.again, new ReviewLog(Rating.Again, this.again.scheduled_days, card.elapsed_days, now, card.state)),
            [Rating.Hard] = new SchedulingInfo(this.hard, new ReviewLog(Rating.Hard, this.hard.scheduled_days, card.elapsed_days, now, card.state)),
            [Rating.Good] = new SchedulingInfo(this.good, new ReviewLog(Rating.Good, this.good.scheduled_days, card.elapsed_days, now, card.state)),
            [Rating.Easy] = new SchedulingInfo(this.easy, new ReviewLog(Rating.Easy, this.easy.scheduled_days, card.elapsed_days, now, card.state)),
        };
    }
}

public class Params
{
    public double request_retention;
    public int maximum_interval;
    public double[] w;

    public Params()
    {
        this.request_retention = 0.9;
        this.maximum_interval = 36500;
        this.w = new double[] { 0.4, 0.6, 2.4, 5.8, 4.93, 0.94, 0.86, 0.01, 1.49, 0.14, 0.94, 2.18, 0.05, 0.34, 1.26, 0.29, 2.61 };
    }
}

public class FSRS
{
    public Params p;

    public FSRS()
    {
        this.p = new Params();
    }

    public Dictionary<Rating, SchedulingInfo> repeat(Card card, DateTime now)
    {
        card = new Card();
        if (card.state == State.New)
        {
            card.elapsed_days = 0;
        }
        else
        {
            card.elapsed_days = (int)(now - card.last_review).TotalDays;
        }
        card.last_review = now;
        card.reps += 1;
        SchedulingCards s = new SchedulingCards(card);
        s.update_state(card.state);
        if (card.state == State.New)
        {
            this.init_ds(s);
            s.again.due = now.AddMinutes(1);
            s.hard.due = now.AddMinutes(5);
            s.good.due = now.AddMinutes(10);
            int easy_interval = this.next_interval(s.easy.stability);
            s.easy.scheduled_days = easy_interval;
            s.easy.due = now.AddDays(easy_interval);
        }
        else if (card.state == State.Learning || card.state == State.Relearning)
        {
            int hard_interval = 0;
            int good_interval = this.next_interval(s.good.stability);
            int easy_interval = Math.Max(this.next_interval(s.easy.stability), good_interval + 1);
            s.schedule(now, hard_interval, good_interval, easy_interval);
        }
        else if (card.state == State.Review)
        {
            int interval = card.elapsed_days;
            double last_d = card.difficulty;
            double last_s = card.stability;
            double retrievability = Math.Pow(1 + interval / (9 * last_s), -1);
            this.next_ds(s, last_d, last_s, retrievability);
            int hard_interval = this.next_interval(s.hard.stability);
            int good_interval = this.next_interval(s.good.stability);
            hard_interval = Math.Min(hard_interval, good_interval);
            good_interval = Math.Max(good_interval, hard_interval + 1);
            int easy_interval = Math.Max(this.next_interval(s.easy.stability), good_interval + 1);
            s.schedule(now, hard_interval, good_interval, easy_interval);
        }
        return s.record_log(card, now);
    }

    public void init_ds(SchedulingCards s)
    {
        s.again.difficulty = this.init_difficulty(Rating.Again);
        s.again.stability = this.init_stability(Rating.Again);
        s.hard.difficulty = this.init_difficulty(Rating.Hard);
        s.hard.stability = this.init_stability(Rating.Hard);
        s.good.difficulty = this.init_difficulty(Rating.Good);
        s.good.stability = this.init_stability(Rating.Good);
        s.easy.difficulty = this.init_difficulty(Rating.Easy);
        s.easy.stability = this.init_stability(Rating.Easy);
    }

    public void next_ds(SchedulingCards s, double last_d, double last_s, double retrievability)
    {
        s.again.difficulty = this.next_difficulty(last_d, Rating.Again);
        s.again.stability = this.next_forget_stability(last_d, last_s, retrievability);
        s.hard.difficulty = this.next_difficulty(last_d, Rating.Hard);
        s.hard.stability = this.next_recall_stability(last_d, last_s, retrievability, Rating.Hard);
        s.good.difficulty = this.next_difficulty(last_d, Rating.Good);
        s.good.stability = this.next_recall_stability(last_d, last_s, retrievability, Rating.Good);
        s.easy.difficulty = this.next_difficulty(last_d, Rating.Easy);
        s.easy.stability = this.next_recall_stability(last_d, last_s, retrievability, Rating.Easy);
    }

    public double init_stability(Rating r)
    {
        return Math.Max(this.p.w[(int)r - 1], 0.1);
    }

    public double init_difficulty(Rating r)
    {
        return Math.Min(Math.Max(this.p.w[4] - this.p.w[5] * ((int)r - 3), 1), 10);
    }

    public int next_interval(double s)
    {
        double interval = s * 9 * (1 / this.p.request_retention - 1);
        return Math.Min(Math.Max((int)Math.Round(interval), 1), this.p.maximum_interval);
    }

    public double next_difficulty(double d, Rating r)
    {
        double next_d = d - this.p.w[6] * ((int)r - 3);
        return Math.Min(Math.Max(this.mean_reversion(this.p.w[4], next_d), 1), 10);
    }

    public double mean_reversion(double init, double current)
    {
        return this.p.w[7] * init + (1 - this.p.w[7]) * current;
    }

    public double next_recall_stability(double d, double s, double r, Rating rating)
    {
        double hard_penalty = rating == Rating.Hard ? this.p.w[15] : 1;
        double easy_bonus = rating == Rating.Easy ? this.p.w[16] : 1;
        return s * (1 + Math.Exp(this.p.w[8]) * (11 - d) * Math.Pow(s, -this.p.w[9]) * (Math.Exp((1 - r) * this.p.w[10]) - 1) * hard_penalty * easy_bonus);
    }

    public double next_forget_stability(double d, double s, double r)
    {
        return this.p.w[11] * Math.Pow(d, -this.p.w[12]) * (Math.Pow(s + 1, this.p.w[13]) - 1) * Math.Exp((1 - r) * this.p.w[14]);
    }
}
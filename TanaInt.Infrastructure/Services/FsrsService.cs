using TanaInt.Domain.Srs.Fsrs;

namespace TanaInt.Infrastructure.Services;

public class FsrsService : IFsrsService
{
    public Card Repeat(FsrsDto dto, DateTime now)
    {
        // TODO: all of this setup needn't to be here, but just due to Tana's limitations.
        var parameters = new Params();
        parameters.MaximumInterval = dto.MaxIntervalInDays ?? parameters.MaximumInterval;
        parameters.RequestRetention = dto.RequestRetention ?? parameters.RequestRetention;

        var card = dto.Card ?? new Card(now);

        // TODO: theoretically, this should never happen, and in case it happen, this should throw an error
        // because we never show a Due card before now.
        // This is just a workaround because of the limitations of Tana not being able to concisely
        // show a due card before now with minute/second granularity, only day.
        if (card.Due > now)
            now = card.Due;

        var result = new FsrsAlgorithm(parameters)
            .Repeat(card, now)[dto.Rating];
        return result.Card;
    }
}

public interface IFsrsService
{
    Card Repeat(FsrsDto dto, DateTime now);
}
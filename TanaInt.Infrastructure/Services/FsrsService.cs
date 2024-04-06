using TanaInt.Domain.Srs.Fsrs;

namespace TanaInt.Infrastructure.Services;

public class FsrsService : IFsrsService
{
    public Card Repeat(FsrsDto dto, DateTime now)
    {
        var parameters = new Params();
        parameters.MaximumInterval = dto.MaxIntervalInDays ?? parameters.MaximumInterval;
        parameters.RequestRetention = dto.RequestRetention ?? parameters.RequestRetention;
        
        var fsrs = new FsrsAlgorithm(parameters);
        var result = fsrs.Repeat(dto.Card ?? new Card(now), DateTime.Now)[dto.Rating];
        return result.Card;
    }
}

public interface IFsrsService
{
    Card Repeat(FsrsDto dto, DateTime now);
}
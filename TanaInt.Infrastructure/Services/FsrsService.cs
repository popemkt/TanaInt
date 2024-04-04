using TanaInt.Domain.Srs.Fsrs;

namespace TanaInt.Infrastructure.Services;

public class FsrsService : IFsrsService
{
    public Card Repeat(FsrsDto dto, DateTime now)
    {
        var fsrs = new Fsrs();
        var result = fsrs.Repeat(dto.Card ?? new Card(), DateTime.Now)[dto.Rating];
        return result.Card;
    }
}

public interface IFsrsService
{
    Card Repeat(FsrsDto dto, DateTime now);
}
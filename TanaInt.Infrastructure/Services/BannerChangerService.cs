using TanaInt.Domain.WallChanger;

namespace TanaInt.Infrastructure.Services;

public interface IBannerChangerService
{
    Task<string> ChangeBanner(BannerChangerDto dto);
}

public class BannerChangerService : IBannerChangerService
{
    public Task<string> ChangeBanner(BannerChangerDto dto)
    {
        if (!dto.ImagesList.Any()) return Task.FromResult(string.Empty);
        
        var randomItemValue = dto.ImagesList[new Random().Next(dto.ImagesList.Count)];
        return Task.FromResult(BannerChangerDto.ParseSingleImageLine(randomItemValue));
    }
}
using System.Net;
using Microsoft.AspNetCore.Mvc;
using TanaInt.Domain.Calendar;
using TanaInt.Domain.Srs.Fsrs;
using TanaInt.Domain.WallChanger;
using TanaInt.Infrastructure.Services;

namespace TanaInt.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{
    [HttpPost("change-wall")]
    public async Task<ActionResult<string>> ChangeWall([FromBody] BannerChangerDto dto,
        [FromServices] IBannerChangerService bannerChangerService)
    {
        try
        {
            return Ok($"{await bannerChangerService.ChangeBanner(dto.ParseImages())}");
        }
        catch
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost("sync-event")]
    public async Task<ActionResult<string>> SyncEvent([FromBody] TanaTaskDto dto,
        [FromServices] IGCalService gCalService)
    {
        try
        {
            return Ok($"{await gCalService.SyncToEvent(dto.ParseInput())}");
        }
        catch
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost("next-rrule")]
    public async Task<ActionResult<string>> NextRRuleOccurence([FromBody] TanaDateTimeDto dto,
        [FromServices] ICalendarHelperService calendarHelper)
    {
        try
        {
            var parsedDto = dto.ParseInput();
            return Ok(TanaDateTimeResponse.FormatDate(
                calendarHelper.NextOccurrence(calendarHelper.ParseRRule(dto.RRule), parsedDto.OccurenceDate)));
        }
        catch
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpPost("fsrs")]
    public async Task<ActionResult<string>> GetFSRs([FromBody] FsrsDto dto,
        [FromServices] IFsrsService fsrsService)
    {
        try
        {
            var parsedDto = dto.ParseInput();
            return Ok(fsrsService.Repeat(parsedDto, DateTime.Now).ToTanaString());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
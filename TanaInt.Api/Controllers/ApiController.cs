using System.Net;
using Microsoft.AspNetCore.Mvc;
using TanaInt.Domain.Calendar;
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
}
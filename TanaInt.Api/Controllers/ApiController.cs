using System.Net;
using Microsoft.AspNetCore.Mvc;
using TanaInt.Domain.Calendar;
using TanaInt.Infrastructure.Services;

namespace TanaInt.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult<string>> SyncEvent([FromBody] TanaTaskDto dto, [FromServices] IGCalService gCalService)
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

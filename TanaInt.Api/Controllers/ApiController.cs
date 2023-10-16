using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using TanaInt.Api.Services;
using TanaInt.Domain;

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
            return Ok($"{await gCalService.SyncEvent(dto.ParseInput())}");
        }
        catch
        {
            return StatusCode((int)HttpStatusCode.InternalServerError);
        }
    }

    [HttpGet]
    public string MockApi()
    {
        return "success";
    }
}
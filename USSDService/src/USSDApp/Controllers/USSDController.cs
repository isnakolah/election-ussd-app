using Microsoft.AspNetCore.Mvc;
using USSDApp.Services;
using USSDTest.DTOs;
using USSDTest.Models;

namespace USSDApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class USSDController : ControllerBase
{
    private readonly USSDService _ussdService;
    private readonly ILogger<USSDController> _logger;

    public USSDController(USSDService ussdService, ILogger<USSDController> logger)
    {
        _ussdService = ussdService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<string>> AfricasTalkingCallback([FromQuery] USSDRequest request)
    {
        _logger.LogInformation("The body is {Request}", request);

        return await _ussdService.GetOptionsAsync(request);
    }
}
using Authentication.Entities;
using Authentication.Model;
using Authentication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Users?>> Register(UserDto request)
    {
        var user = await _service.RegisterAsync(request);

        if (user is null)
            return BadRequest("User already exists");

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
    {
        var token = await _service.LoginAsync(request);

        if (token is null)
            return Unauthorized("Invalid username or password");

        return Ok(token);
    }
    
    [HttpPost("RefreshToken")]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenDto request)
    {
        var token = await _service.RefreshTokenAsync(request);

        if (token is null)
            return Unauthorized("Invalid username or password");

        return Ok(token);
    }

    [HttpGet("Auth-endpoint")]
    [Authorize]
    public ActionResult AuthCheck()
    {
        return Ok();
        
    }
    
    [HttpGet("Admin-endpoint")]
    [Authorize(Roles = "Admin")]
    public ActionResult AdminCheck()
    {
        return Ok();
    }
}
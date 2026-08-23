using HasbitFlowApi.DTOs.Auth;
using HasbitFlowApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HasbitFlowApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _userService.RegisterAsync(dto);

            if (!result)
            {
                return Conflict(new
                {
                    message = "Email Already Exists"
                });
            }

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _userService.LoginAsync(dto);

            if (token is null)
            {
                return Unauthorized(new
                {
                    message = "Invalid Email or Password"
                });
            }

            return Ok(new
            {
                accessToken = token
            });
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> GetMe()
        {
            //var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
                return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId);

            if (user is null)
                return NotFound();

            return Ok(new
            {
                user.Id,
                user.Name,
                user.Email,
                user.CreatedAt
            });
        }
    }
}

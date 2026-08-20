using HasbitFlowApi.DTOs.Auth;
using HasbitFlowApi.Services;
using Microsoft.AspNetCore.Mvc;

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
            var result = await _userService.LoginAsync(dto);

            if (!result)
            {
                return Unauthorized(new
                {
                    message = "Invalid Email or Password"
                });
            }

            return Ok(new
            {
                message = "Login successful"
            });
        }
    }
}

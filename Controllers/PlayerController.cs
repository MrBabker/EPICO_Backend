using epico_backend.Controllers.Interfaces;
using epico_backend.DTOs;
using epico_backend.models.players;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace epico_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerController : ControllerBase
    {
        private bool isSecured = true;
        private readonly ILogger<PlayerController> _logger;
        private readonly IPlayerServices _playerServices;
        private readonly IJwtServices _jwtServices;
        public PlayerController( ILogger<PlayerController> logger, IPlayerServices playerServices , IJwtServices jwtServices) 
        { 
            _logger = logger;
            _playerServices = playerServices;
            _jwtServices = jwtServices;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPlayers()
        {
            var players = await _playerServices.GetAllPlayers();
            return Ok(players.Select(p => new
            {
                p.Name,
                p.Points,
                p.Level
            }));
        }

        [HttpGet("top")]
        public async Task<IActionResult> Get10Players()
        {
            var players = await _playerServices.Get10Players();

            return Ok(players.Select(p => new
            {
                p.Name,
                p.Points,
                p.Level
            }));
        }
        [Authorize]
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userId, out int payloadId))
            {
                return Unauthorized("Invalid Token");
            }

            var player = await _playerServices.GetCurrentUser(payloadId);

            if (player == null)
            {
                return NotFound("User Not Found");
            }

            return Ok(new
            {
                name = player.Name,
                username = player.UserName,
                email = player.Email,
                points = player.Points,
                level = player.Level
            });
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreatePlayer([FromBody] CreatePlayerDTOO DTO)
        {
            var checkName = await _playerServices.CheckName(DTO.Name);
            if (checkName) return Conflict("This name is used !");

            var checkEmail = await _playerServices.CheckEmail(DTO.Email);
            if (checkEmail) return Conflict("This email is already used !");

            var player = await _playerServices.CreatePlayer(DTO);
            if (player == null) return BadRequest();

            var token = _jwtServices.CreateToken(player);

            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecured,
                Path="/",
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            return Ok(new
            {
                message = player.Name + " created sucscsusflly !!",
                player = new
                {
                    player.Id,
                    player.Name,
                    player.UserName,
                    player.Email,
                    player.Points,
                    player.Level,
                },

            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO DTO)
        {
            var player = await _playerServices.Login(DTO);

            if (player == null)
                return Unauthorized("Wrong email or password");

            string token = _jwtServices.CreateToken(player);

            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = isSecured,
                Path="/",
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });

            return Ok(new
            {
                player = new
                {
                    player.Id,
                    player.Name,
                    player.UserName,
                    player.Email,
                    player.Points,
                    player.Level,
                },
                token = token
            });
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token", new CookieOptions
            {
                HttpOnly = true,
                Path="/",
                Secure = isSecured,
                SameSite = SameSiteMode.None,
            });

            return Ok(new
            {
                message = "Logged out successfully"
            });
        }

        [HttpPost("update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO DTO)
        {
            var result = await _playerServices.UpdatePassword(DTO);

            if (result == false)
                return BadRequest("Invalid token");

            return Ok("Password updated successfully");
        }


        [Authorize]
        [HttpPost("add-points/{amount}")]
        public async Task<IActionResult> AddPoints(int amount)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var player = await _playerServices.AddPoints(
                int.Parse(userId),
                amount);

            if (player == null)
                return NotFound();

            return Ok(new
            {
                player.Points
            });
        }
    }
}

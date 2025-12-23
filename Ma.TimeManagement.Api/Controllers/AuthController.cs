using Ma.TimeManagement.Models;
using Ma.TimeManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ma.TimeManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public AuthController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [HttpGet("whoami")]
        [Authorize]
        public IActionResult WhoAmI()
        {
            return Ok(new
            {
                User = User.Identity?.Name,
                AuthType = User.Identity?.AuthenticationType,
                IsAuthenticated = User.Identity?.IsAuthenticated
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResult>> Login([FromBody] LoginModel model, CancellationToken cancellationToken)
        {
            var user = await _userService.ValidateUser(model.UserID, cancellationToken);
            if (user == null) return Unauthorized();

            var token = GenerateJwt(user);

            bool hasPat = !string.IsNullOrEmpty(await _userService.GetPatAsync(user.Id, cancellationToken));

            return Ok(new LoginResult
            {
                Token = token,
                PatConfigured = hasPat
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResult>> Register([FromBody] RegisterModel model, CancellationToken cancellationToken)
        {
            User user =
            await _userService.IsUserExistAsync(model.Username, cancellationToken) ?
                 await _userService.GetUserAsync(model.Username, cancellationToken) :
                await _userService.CreateUserAsync(model.Username, model.PAT, cancellationToken);
            
            if (user == null) return Unauthorized();

            await _userService.SavePatAsync(user.Id,model.PAT,cancellationToken);
            
            return Ok(new RegisterResult
            {
                UserID = user.Id
            });
        }

        [HttpPost("save-pat")]
        [Authorize]
        public async Task<IActionResult> SavePat([FromBody] SavePatModel model, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _userService.SavePatAsync(userId, model.Pat, cancellationToken);
            return Ok(new { message = "PAT saved securely" });
        }

        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "sdfgtryeu546okfdjfgvnhgitdfugtre"))
            {
                KeyId = "LocalServerKey"
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds,
                Issuer = null,
                Audience = null
            };

            return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
        }
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryProject.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LibraryProject.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<AppUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }

        public record LoginRequest(string Email, string Password);
        public record LoginResponse(string Token, string FullName);

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized(new { message = "E-posta veya şifre hatalı" });

            if (await _userManager.IsLockedOutAsync(user))
                return Unauthorized(new { message = "Bu hesap devre dışı bırakılmış" });

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized(new { message = "E-posta veya şifre hatalı" });

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Visitor"))
                return Unauthorized(new { message = "Mobil uygulama yalnızca ziyaretçiler içindir" });

            // Bileti bas
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, "Visitor")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(
                    int.Parse(_config["Jwt:ExpireDays"]!)),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256));

            return Ok(new LoginResponse(
                new JwtSecurityTokenHandler().WriteToken(token),
                user.FullName));
        }
    }
}
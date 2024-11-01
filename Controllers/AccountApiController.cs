using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Store.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;

namespace Store.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;

        public AccountApiController(
            UserManager<ApplicationUser> usrMgr,
            SignInManager<ApplicationUser> sgnMgr
        )
        {
            userManager = usrMgr;
            signInManager = sgnMgr;
        }

        [HttpPost("token")]
        public async Task<IActionResult> Token(
            [FromBody] Credentials credentials
        )
        {
            string? username = credentials.Username;
            string? password = credentials.Password;

            ApplicationUser? user = await userManager.FindByNameAsync(username);

            if (user != null && (await signInManager
                .CheckPasswordSignInAsync(user, password, true))
                .Succeeded
            )
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();

                byte[] secret = Encoding.ASCII
                    .GetBytes(Environment.GetEnvironmentVariable("jwtSecret")!);
                SecurityTokenDescriptor desk = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, username)
                    }),
                    Expires = DateTime.UtcNow.AddHours(24),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(secret),
                        SecurityAlgorithms.HmacSha256Signature
                    )
                };

                SecurityToken token = handler.CreateToken(desk);
                return Ok(new
                {
                    success = true,
                    token = handler.WriteToken(token)
                });
            }
            return Unauthorized();
        }

        public class Credentials
        {
            public string Username { get; set; } = String.Empty;
            public string Password { get; set; } = String.Empty;
        }
    }
}

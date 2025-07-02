using Beauvoir.DTO;
using Beauvoir.Models;
using Beauvoir.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Beauvoir.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _DbContext;

        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _DbContext = context;
        }

        [HttpPost("[action]")]
        public ActionResult<UserDto> Register([FromBody] RegisterDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("User data is required.");
            }
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var trimmedUsername = userDto.Username?.Trim();
            if (string.IsNullOrEmpty(trimmedUsername))
                return BadRequest("Username is required.");

            try
            {
                
                // Check if there is such a username in the database already

                if (_DbContext.Users.Any(x => x.Username.Equals(trimmedUsername)))
                    return BadRequest($"Username {trimmedUsername} already exists");

                // Hash the password
                var b64salt = PasswordHashProvider.GetSalt();
                var b64hash = PasswordHashProvider.GetHash(userDto.Password, b64salt);

                // Create user from DTO and hashed password
                var user = new User
                {

                    Username = trimmedUsername,
                    PwdHash = b64hash,
                    PwdSalt = b64salt,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email
                };

                // Add user and save Changes to database
                _DbContext.Add(user);
                _DbContext.SaveChanges();

                // Update DTO Id to return it to the client


                return Ok(new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = userDto.Password,
                    Email = user.Email
                }); 

            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while registering the user :" + ex.Message);
            }
        }

        [HttpPost("[action]")]
        public ActionResult Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                var genericLoginFail = "Incorrect username or password";

                // Try to get a user from database
                var existingUser = _DbContext.Users.FirstOrDefault(x => x.Username == loginDto.Username);
                if (existingUser == null)
                    return BadRequest(genericLoginFail);

                // Check is password hash matches
                var b64hash = PasswordHashProvider.GetHash(loginDto.Password, existingUser.PwdSalt);
                if (b64hash != existingUser.PwdHash)
                    return BadRequest(genericLoginFail);

                var secureKey = _configuration["JWT:SecureKey"];
                var serializedToken = JwtTokenProvider.CreateToken(secureKey, 120, loginDto.Username);

                return Ok(serializedToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("[action]")]
        [Authorize]
        public ActionResult Me()
        {
            try
            {
                // Obtener el nombre de usuario desde el JWT (ClaimTypes.Name)
                var username = User.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                    return Unauthorized("Token does not contain username.");

                // Buscar el usuario por nombre de usuario
                var user = _DbContext.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                    return NotFound("User not found.");

                return Ok (new 
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.FirstName,
                    user.LastName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error retrieving user: " + ex.Message);
            }
        }



    }
}

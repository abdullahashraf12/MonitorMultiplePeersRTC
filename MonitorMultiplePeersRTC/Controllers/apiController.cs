using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MonitorMultiplePeersRTC.Models.UserRTCModel; // Import the UserRTCModel class
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MonitorMultiplePeersRTC.Models;
using MonitorMultiplePeersRTC.Data;
namespace MonitorMultiplePeersRTC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class authController : ControllerBase
    {
        private readonly HttpClient _client;
        private readonly ILogger<authController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public authController(HttpClient client, ILogger<authController> logger, ApplicationDbContext dbContext)
        {
            _client = client;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                // Check if the user exists in the system by username (email)
                var existingUser = _dbContext.UserRTCModel.FirstOrDefault(u => u.Email == loginRequest.Username);

                if (existingUser == null)
                {
                    _logger.LogError("Error: User with email {Email} does not exist.", loginRequest.Username);
                    return Unauthorized(new { message = "Authentication failed. User does not exist." });
                }

                // Decrypt the stored password
                var decryptedPasswordfromdatabase = AES256Encryption.Decrypt(existingUser.Password);
                var decryptedPasswordfromClient = AES256Encryption.Decrypt(loginRequest.Password);

                //var decryptedPassword = AES256Encryption.Decrypt(existingUser.Password);

                // Check if the provided password matches the decrypted password
                if (decryptedPasswordfromClient != decryptedPasswordfromdatabase)
                {

                    return Unauthorized(new { message = "Authentication failed. Invalid password." });
                }
                else
                {

                    // If user exists and password matches, return the Token from the model
                    _logger.LogInformation("User {Email} logged in successfully.", existingUser.Token);
                    // Initialize the dictionary correctly
                    Dictionary<string, string> d = new Dictionary<string, string>();
                    d.Add("token", existingUser.Token);

                    // Return the dictionary as JSON
                    return Ok(d);

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the login.");
                return StatusCode(500, new { message = "An error occurred while processing your request. Please try again." });
            }
        }
    }

    // Define the LoginRequest model for deserializing the incoming request body
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

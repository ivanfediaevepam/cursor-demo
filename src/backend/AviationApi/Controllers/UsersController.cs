using AviationApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AviationApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    public UsersController(ILogger<UsersController> logger)
    {
        _logger = logger;
    }

    [HttpGet("{id}")]
    public ActionResult<User> GetById(int id)
    {
        _logger.LogInformation($"GET User {id}");

        // For demo purposes, we'll just return a mock user
        // SECURITY ISSUE: We are returning the PasswordHash field in the API response
        var user = new User
        {
            Id = id,
            Username = "jdoe",
            Email = "john.doe@example.com",
            PasswordHash = "SHA256:5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8" 
        };

        return Ok(user);
    }
}

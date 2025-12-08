using Microsoft.AspNetCore.Mvc;

namespace Project420.API.WebApi.Controllers;

/// <summary>
/// Authentication and customer registration endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Register a new customer account
    /// </summary>
    /// <remarks>
    /// Creates a new customer account with age verification.
    /// Customer must be 18+ (Cannabis Act requirement).
    /// POPIA consent is mandatory.
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register()
    {
        // TODO: Implement customer registration
        // - Validate age (18+)
        // - Verify POPIA consent
        // - Hash password
        // - Send email verification
        return Ok(new { message = "Registration endpoint - to be implemented" });
    }

    /// <summary>
    /// Customer login
    /// </summary>
    /// <remarks>
    /// Authenticates customer and returns JWT token
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login()
    {
        // TODO: Implement login
        // - Validate credentials
        // - Generate JWT token
        // - Return token + refresh token
        return Ok(new { message = "Login endpoint - to be implemented" });
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
        // TODO: Implement token refresh
        return Ok(new { message = "Refresh token endpoint - to be implemented" });
    }

    /// <summary>
    /// Verify customer age with ID document
    /// </summary>
    /// <remarks>
    /// Required before placing first order (Cannabis Act compliance)
    /// </remarks>
    [HttpPost("verify-age")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyAge()
    {
        // TODO: Implement age verification
        // - Validate ID number
        // - Extract date of birth
        // - Confirm 18+
        // - Store verification record
        return Ok(new { message = "Age verification endpoint - to be implemented" });
    }
}

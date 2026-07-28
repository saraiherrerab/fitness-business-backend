using System.Security.Claims;
using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Auth;
using FitwomanAPI.Models;
using FitwomanAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Registers a new administrative user account
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var emailExists = await _context.Usuarios
            .AnyAsync(u => u.Correo.ToLower() == dto.Email.ToLower());

        if (emailExists)
        {
            return BadRequest(new { message = "Email is already registered." });
        }

        // Secure password hashing with BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var user = new Usuario
        {
            Nombre = dto.FirstName,
            Apellido = dto.LastName,
            Correo = dto.Email.ToLower(),
            Contraseña = passwordHash
        };

        _context.Usuarios.Add(user);
        await _context.SaveChangesAsync();

        var response = new RegisterResponseDto
        {
            Message = "User registered successfully.",
            User = new UserProfileDto
            {
                Id = user.IdUsuarios,
                FirstName = user.Nombre,
                LastName = user.Apellido,
                Email = user.Correo,
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "Admin" : dto.Role
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// User authentication / Login
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var user = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == dto.Email.ToLower());

        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        // Verify password (supports BCrypt hashes and automatic migration if legacy plain text)
        bool isValid = false;
        if (user.Contraseña.StartsWith("$2a$") || user.Contraseña.StartsWith("$2b$") || user.Contraseña.StartsWith("$2y$"))
        {
            isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Contraseña);
        }
        else
        {
            if (user.Contraseña == dto.Password)
            {
                isValid = true;
                user.Contraseña = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                await _context.SaveChangesAsync();
            }
        }

        if (!isValid)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var (token, expiration) = _jwtService.GenerateToken(user, "Admin");

        // Save token in HttpOnly Cookie for XSS security
        _jwtService.SetTokenCookie(Response, token, expiration);

        var response = new LoginResponseDto
        {
            Token = token,
            Expiration = expiration,
            User = new UserProfileDto
            {
                Id = user.IdUsuarios,
                FirstName = user.Nombre,
                LastName = user.Apellido,
                Email = user.Correo,
                Role = "Admin"
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Logout current session (Clears cookies)
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        _jwtService.ClearTokenCookie(Response);
        return Ok(new { message = "Session closed successfully." });
    }

    /// <summary>
    /// Retrieves current authenticated user profile
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid or expired token." });
        }

        var user = await _context.Usuarios.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        var profile = new UserProfileDto
        {
            Id = user.IdUsuarios,
            FirstName = user.Nombre,
            LastName = user.Apellido,
            Email = user.Correo,
            Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Admin"
        };

        return Ok(profile);
    }
}

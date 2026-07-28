using System.Security.Claims;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Auth;
using FitwomanAPI.Models;
using FitwomanAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// Registro de nuevo usuario administrativo
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var existeCorreo = await _context.Usuarios
            .AnyAsync(u => u.Correo.ToLower() == dto.Correo.ToLower());

        if (existeCorreo)
        {
            return BadRequest(new { message = "El correo ya se encuentra registrado." });
        }

        // Hashing seguro de contraseña con BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Contraseña);

        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Correo = dto.Correo.ToLower(),
            Contraseña = passwordHash
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var response = new RegisterResponseDto
        {
            Message = "Usuario registrado exitosamente.",
            User = new UserProfileDto
            {
                Id = usuario.IdUsuarios,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                Rol = string.IsNullOrWhiteSpace(dto.Rol) ? "Admin" : dto.Rol
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Autenticación de usuario / Inicio de sesión
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == dto.Correo.ToLower());

        if (usuario == null)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        // Verificar la contraseña (soporta contraseñas antiguas en texto plano o hashes BCrypt)
        bool esValida = false;
        if (usuario.Contraseña.StartsWith("$2a$") || usuario.Contraseña.StartsWith("$2b$") || usuario.Contraseña.StartsWith("$2y$"))
        {
            esValida = BCrypt.Net.BCrypt.Verify(dto.Contraseña, usuario.Contraseña);
        }
        else
        {
            // Migración transparente si la contraseña estaba guardada en texto plano
            if (usuario.Contraseña == dto.Contraseña)
            {
                esValida = true;
                usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(dto.Contraseña);
                await _context.SaveChangesAsync();
            }
        }

        if (!esValida)
        {
            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var (token, expiration) = _jwtService.GenerateToken(usuario, "Admin");

        // Guardar token en cookie HttpOnly por seguridad XSS
        _jwtService.SetTokenCookie(Response, token, expiration);

        var response = new LoginResponseDto
        {
            Token = token,
            Expiration = expiration,
            User = new UserProfileDto
            {
                Id = usuario.IdUsuarios,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                Rol = "Admin"
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Cierre de sesión (Limpia cookies)
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _jwtService.ClearTokenCookie(Response);
        return Ok(new { message = "Sesión cerrada correctamente." });
    }

    /// <summary>
    /// Obtiene los datos del usuario actualmente autenticado a partir de su JWT Token
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUserProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido o expirado." });
        }

        var usuario = await _context.Usuarios.FindAsync(userId);
        if (usuario == null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        var perfil = new UserProfileDto
        {
            Id = usuario.IdUsuarios,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Correo = usuario.Correo,
            Rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "Admin"
        };

        return Ok(perfil);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitwomanAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace FitwomanAPI.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime Expiration) GenerateToken(Usuario usuario, string rol = "Admin")
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey no está configurada.");
        var issuer = _configuration["Jwt:Issuer"] ?? "FitwomanAPI";
        var audience = _configuration["Jwt:Audience"] ?? "FitwomanApps";
        
        var expirationMinutesStr = _configuration["Jwt:AccessTokenExpirationMinutes"];
        var expirationMinutes = int.TryParse(expirationMinutesStr, out var minutes) ? minutes : 30;

        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuarios.ToString()),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.GivenName, usuario.Nombre),
            new Claim(ClaimTypes.Surname, usuario.Apellido),
            new Claim(ClaimTypes.Role, rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiration);
    }

    public void SetTokenCookie(HttpResponse response, string token, DateTime expiration)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Exige HTTPS en producción
            SameSite = SameSiteMode.Strict, // O SameSiteMode.None si están en dominios completamente distintos con HTTPS
            Expires = expiration,
            Path = "/"
        };

        response.Cookies.Append("access_token", token, cookieOptions);
    }

    public void ClearTokenCookie(HttpResponse response)
    {
        response.Cookies.Delete("access_token");
    }
}

using FitwomanAPI.Models;
using Microsoft.AspNetCore.Http;

namespace FitwomanAPI.Services;

public interface IJwtService
{
    (string Token, DateTime Expiration) GenerateToken(Usuario usuario, string rol = "Admin");
    void SetTokenCookie(HttpResponse response, string token, DateTime expiration);
    void ClearTokenCookie(HttpResponse response);
}

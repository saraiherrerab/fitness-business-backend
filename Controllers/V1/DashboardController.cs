using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene las métricas y estadísticas consolidadas para el Dashboard del Portal Admin
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardSummary()
    {
        // 1. Contador de miembros activos
        var activeMembers = await _context.Miembros
            .Where(m => m.Estado == null || m.Estado.ToLower() == "active" || m.Estado.ToLower() == "activo")
            .CountAsync();

        // 2. Contador de clases publicadas
        var publishedClasses = await _context.Clases.CountAsync();

        // 3. Contador de productos en tienda
        var storeProducts = await _context.Productos
            .Where(p => p.Visibilidad)
            .CountAsync();

        // 4. Productos agregados este mes
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var newProductsThisMonth = await _context.Productos
            .Where(p => p.FechaRegistro >= startOfMonth)
            .CountAsync();

        // 5. Crecimiento de miembros en los últimos 6 meses
        var sixMonthsAgo = startOfMonth.AddMonths(-5);
        var recentMembers = await _context.Miembros
            .Where(m => m.FechaIngreso >= sixMonthsAgo)
            .ToListAsync();

        var monthlyGrowth = new List<MonthlyMemberGrowthDto>();
        for (int i = 5; i >= 0; i--)
        {
            var targetMonthDate = startOfMonth.AddMonths(-i);
            var monthName = targetMonthDate.ToString("MMM");
            var countInMonth = recentMembers.Count(m =>
                m.FechaIngreso.Year == targetMonthDate.Year &&
                m.FechaIngreso.Month == targetMonthDate.Month);

            monthlyGrowth.Add(new MonthlyMemberGrowthDto
            {
                Month = char.ToUpper(monthName[0]) + monthName.Substring(1),
                Count = countInMonth
            });
        }

        // 6. Distribución de Clases por Tipo (para gráfico de dona)
        var classGroups = await _context.Clases
            .GroupBy(c => c.Tipo)
            .Select(g => new { Tipo = g.Key, Count = g.Count() })
            .ToListAsync();

        var totalClassesCount = publishedClasses > 0 ? publishedClasses : 1;
        var classDistribution = classGroups.Select(g => new ClassDistributionDto
        {
            ClassType = g.Tipo,
            Count = g.Count,
            Percentage = Math.Round(((double)g.Count / totalClassesCount) * 100, 1)
        }).ToList();

        var result = new DashboardSummaryDto
        {
            ActiveMembersCount = activeMembers,
            ActiveMembersGrowthPercentage = 12.0, // Crecimiento mensual estimado
            PublishedClassesCount = publishedClasses,
            StoreProductsCount = storeProducts,
            NewProductsThisMonth = newProductsThisMonth,
            MonthlyMembersGrowth = monthlyGrowth,
            ClassDistribution = classDistribution
        };

        return Ok(result);
    }
}

namespace FitwomanAPI.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int ActiveMembersCount { get; set; }
    public double ActiveMembersGrowthPercentage { get; set; }
    public int PublishedClassesCount { get; set; }
    public int StoreProductsCount { get; set; }
    public int NewProductsThisMonth { get; set; }
    public List<MonthlyMemberGrowthDto> MonthlyMembersGrowth { get; set; } = new();
    public List<ClassDistributionDto> ClassDistribution { get; set; } = new();
}

public class MonthlyMemberGrowthDto
{
    public string Month { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ClassDistributionDto
{
    public string ClassType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

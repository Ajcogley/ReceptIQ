namespace ReciptIQ.DTOs.Dashboard;

public class DashboardStatsDto
{
    public decimal TotalSpentThisMonth { get; set; }
    public int TotalReceiptsThisMonth { get; set; }
    public int PendingApprovals { get; set; }
    public decimal BudgetUsedPercentage { get; set; }
    public List<CategoryExpenseDto> TopCategories { get; set; } = new();
    public List<DailyExpenseDto> DailyExpenses { get; set; } = new();
}

public class CategoryExpenseDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class DailyExpenseDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}

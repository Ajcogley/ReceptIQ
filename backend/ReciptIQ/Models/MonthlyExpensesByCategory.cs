using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

public partial class MonthlyExpensesByCategory
{
    public Guid? CompanyId { get; set; }

    public string? CategoryName { get; set; }

    public string? CategoryColor { get; set; }

    public decimal? Year { get; set; }

    public decimal? Month { get; set; }

    public long? ReceiptCount { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? AvgAmount { get; set; }
}

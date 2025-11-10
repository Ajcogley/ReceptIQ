using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

public partial class DashboardStat
{
    public Guid? CompanyId { get; set; }

    public Guid? UserId { get; set; }

    public decimal? Year { get; set; }

    public decimal? Month { get; set; }

    public long? TotalReceipts { get; set; }

    public long? PendingReceipts { get; set; }

    public long? ApprovedReceipts { get; set; }

    public decimal? TotalSpent { get; set; }

    public decimal? ApprovedSpent { get; set; }
}

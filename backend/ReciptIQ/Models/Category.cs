using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Categorías de gastos personalizables por empresa
/// </summary>
public partial class Category
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Color { get; set; }

    public string? Icon { get; set; }

    public decimal? BudgetMonthly { get; set; }

    /// <summary>
    /// true para categorías del sistema (Comida, Transporte, etc.)
    /// </summary>
    public bool? IsDefault { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();

    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
}

using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Presupuestos mensuales por categoría y/o usuario
/// </summary>
public partial class Budget
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? CategoryId { get; set; }

    /// <summary>
    /// NULL = presupuesto de empresa completa, UUID = presupuesto individual
    /// </summary>
    public Guid? UserId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal BudgetAmount { get; set; }

    public decimal? SpentAmount { get; set; }

    public decimal? RemainingAmount { get; set; }

    public decimal? AlertThreshold { get; set; }

    public bool? AlertSent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual User? User { get; set; }
}

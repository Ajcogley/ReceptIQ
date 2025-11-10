using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Items individuales extraídos de cada recibo para análisis detallado
/// </summary>
public partial class ReceiptItem
{
    public Guid Id { get; set; }

    public Guid ReceiptId { get; set; }

    public int ItemNumber { get; set; }

    public string Description { get; set; } = null!;

    public decimal? Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public Guid? CategoryId { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Receipt Receipt { get; set; } = null!;
}

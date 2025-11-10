using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Proveedores/comercios extraídos automáticamente de recibos
/// </summary>
public partial class Vendor
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>
    /// Normalizado para evitar duplicados (UPPER, sin espacios extra)
    /// </summary>
    public string? NormalizedName { get; set; }

    public Guid? CategoryId { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public string? Address { get; set; }

    public string? TaxId { get; set; }

    public string? LogoUrl { get; set; }

    public bool? IsVerified { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
}

using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Empresas u organizaciones que usan la plataforma
/// </summary>
public partial class Company
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Industry { get; set; }

    public string? TaxId { get; set; }

    public string? LogoUrl { get; set; }

    /// <summary>
    /// Configuraciones JSON: políticas de gastos, límites, notificaciones
    /// </summary>
    public string? Settings { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
}

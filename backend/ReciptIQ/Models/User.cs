using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Usuarios de la plataforma con roles y permisos
/// </summary>
public partial class User
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    /// <summary>
    /// admin: control total, manager: aprueba gastos, employee: solo sube recibos
    /// </summary>
    public string Role { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public string? Phone { get; set; }

    public string? Department { get; set; }

    public bool? IsActive { get; set; }

    public bool? EmailVerified { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Receipt> ReceiptApprovedByNavigations { get; set; } = new List<Receipt>();

    public virtual ICollection<Receipt> ReceiptUsers { get; set; } = new List<Receipt>();

    public virtual ICollection<SystemSetting> SystemSettings { get; set; } = new List<SystemSetting>();
}

using System;
using System.Collections.Generic;
using System.Net;

namespace ReciptIQ.Models;

/// <summary>
/// Auditoría completa de todas las operaciones críticas
/// </summary>
public partial class AuditLog
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string EntityType { get; set; } = null!;

    public Guid EntityId { get; set; }

    public string Action { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public IPAddress? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}

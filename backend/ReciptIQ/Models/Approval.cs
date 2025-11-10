using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Historial completo de aprobaciones y rechazos con comentarios
/// </summary>
public partial class Approval
{
    public Guid Id { get; set; }

    public Guid ReceiptId { get; set; }

    public Guid ApproverId { get; set; }

    public string Action { get; set; } = null!;

    public string? Comments { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Approver { get; set; } = null!;

    public virtual Receipt Receipt { get; set; } = null!;
}

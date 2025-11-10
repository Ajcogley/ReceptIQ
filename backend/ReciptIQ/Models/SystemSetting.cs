using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Configuraciones globales del sistema (límites de API, features flags, etc.)
/// </summary>
public partial class SystemSetting
{
    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}

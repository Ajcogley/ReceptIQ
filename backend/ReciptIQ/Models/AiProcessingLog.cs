using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Logs de todas las llamadas a APIs de IA para debugging y auditoría
/// </summary>
public partial class AiProcessingLog
{
    public Guid Id { get; set; }

    public Guid ReceiptId { get; set; }

    public string Provider { get; set; } = null!;

    public string Model { get; set; } = null!;

    public int? TokensUsed { get; set; }

    public int? ProcessingTimeMs { get; set; }

    public string? RawResponse { get; set; }

    public bool ExtractionSuccessful { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Receipt Receipt { get; set; } = null!;
}

using System;
using System.Collections.Generic;

namespace ReciptIQ.Models;

/// <summary>
/// Recibos/facturas con datos extraídos por IA
/// </summary>
public partial class Receipt
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Guid UserId { get; set; }

    public Guid? VendorId { get; set; }

    public Guid? CategoryId { get; set; }

    public string? ReceiptNumber { get; set; }

    public DateOnly ReceiptDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Currency { get; set; }

    public decimal? TaxAmount { get; set; }

    public decimal? TipAmount { get; set; }

    public string? VendorName { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? OriginalFilename { get; set; }

    public int? FileSizeBytes { get; set; }

    public string? MimeType { get; set; }

    /// <summary>
    /// JSON con todos los campos extraídos: items, subtotales, métodos de pago, etc.
    /// </summary>
    public string? ExtractedData { get; set; }

    public string Status { get; set; } = null!;

    public string? ProcessingStatus { get; set; }

    public bool? IsAiProcessed { get; set; }

    /// <summary>
    /// Nivel de confianza de la IA (0-1). &lt;0.7 requiere revisión manual
    /// </summary>
    public decimal? ConfidenceScore { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Tags personalizados para organización flexible
    /// </summary>
    public List<string>? Tags { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public bool? IsDuplicate { get; set; }

    public bool? IsFlagged { get; set; }

    public string? FlagReason { get; set; }

    public virtual ICollection<AiProcessingLog> AiProcessingLogs { get; set; } = new List<AiProcessingLog>();

    public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();

    public virtual User? ApprovedByNavigation { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();

    public virtual User User { get; set; } = null!;

    public virtual Vendor? Vendor { get; set; }
}

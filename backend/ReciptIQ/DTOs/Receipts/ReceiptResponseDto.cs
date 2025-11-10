namespace ReciptIQ.DTOs.Receipts;

public class ReceiptResponseDto
{
    public Guid Id { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? VendorName { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryColor { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAiProcessed { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

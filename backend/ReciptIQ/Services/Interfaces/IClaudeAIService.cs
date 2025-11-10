namespace ReciptIQ.Services.Interfaces;

public interface IClaudeAIService
{
    Task<ReceiptExtractionResult> ExtractReceiptDataAsync(string imageUrl);
}

public class ReceiptExtractionResult
{
    public string? VendorName { get; set; }
    public DateOnly? ReceiptDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public string? SuggestedCategory { get; set; }
    public decimal ConfidenceScore { get; set; }
    public List<ReceiptItemData> Items { get; set; } = new();
}

public class ReceiptItemData
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

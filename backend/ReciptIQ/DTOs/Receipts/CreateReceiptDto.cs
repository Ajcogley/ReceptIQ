namespace ReciptIQ.DTOs.Receipts;

public class CreateReceiptDto
{
    public DateTime? ReceiptDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? VendorName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Notes { get; set; }
}

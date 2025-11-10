
using ReciptIQ.DTOs.Receipts;

namespace ReciptIQ.API.Services.Interfaces;

public interface IReceiptService
{
    Task<ReceiptResponseDto> CreateReceiptAsync(Guid userId, IFormFile file, CreateReceiptDto? dto = null);
    Task<ReceiptResponseDto?> GetReceiptByIdAsync(Guid receiptId, Guid userId);
    Task<(List<ReceiptResponseDto> Receipts, int TotalCount)> GetUserReceiptsAsync(
        Guid userId, int page, int pageSize, string? status = null, Guid? categoryId = null);
    Task<ReceiptResponseDto> UpdateReceiptAsync(Guid receiptId, Guid userId, CreateReceiptDto dto);
    Task<bool> DeleteReceiptAsync(Guid receiptId, Guid userId);
    Task ProcessReceiptWithAIAsync(Guid receiptId);
}

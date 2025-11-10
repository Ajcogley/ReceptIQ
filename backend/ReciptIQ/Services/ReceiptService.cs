using Microsoft.EntityFrameworkCore;
using ReciptIQ.API.Services.Interfaces;
using ReciptIQ.DTOs.Receipts;
using ReciptIQ.Models;
using ReciptIQ.Services.Interfaces;

namespace ReciptIQ.API.Services.Implementations;

public class ReceiptService : IReceiptService
{
    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorage;
    private readonly IClaudeAIService _aiService;

    public ReceiptService(AppDbContext context, IFileStorageService fileStorage, IClaudeAIService aiService)
    {
        _context = context;
        _fileStorage = fileStorage;
        _aiService = aiService;
    }

    public async Task<ReceiptResponseDto> CreateReceiptAsync(Guid userId, IFormFile file, CreateReceiptDto? dto = null)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new UnauthorizedAccessException("Usuario no encontrado");

        // Guardar archivo
        var filePath = await _fileStorage.SaveFileAsync(file);
        var imageUrl = _fileStorage.GetFileUrl(filePath);

        // Crear recibo
        var receipt = new Receipt
        {
            Id = Guid.NewGuid(),
            CompanyId = user.CompanyId,
            UserId = userId,
            ReceiptDate = dto?.ReceiptDate != null
            ? DateOnly.FromDateTime(dto.ReceiptDate.Value)
            : DateOnly.FromDateTime(DateTime.Now),

            TotalAmount = dto?.TotalAmount ?? 0,
            VendorName = dto?.VendorName,
            CategoryId = dto?.CategoryId,
            ImageUrl = imageUrl,
            OriginalFilename = file.FileName,
            FileSizeBytes = (int)file.Length,
            MimeType = file.ContentType,
            Status = "pending",
            ProcessingStatus = "uploaded",
            IsAiProcessed = false,
            Notes = dto?.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();

        // Procesar con IA en background (no bloquear respuesta)
        _ = Task.Run(() => ProcessReceiptWithAIAsync(receipt.Id));

        return await MapToDto(receipt);
    }

    public async Task ProcessReceiptWithAIAsync(Guid receiptId)
    {
        try
        {
            var receipt = await _context.Receipts.FindAsync(receiptId);
            if (receipt == null) return;

            receipt.ProcessingStatus = "processing";
            await _context.SaveChangesAsync();

            // Llamar a Claude API
            var extractedData = await _aiService.ExtractReceiptDataAsync(receipt.ImageUrl);

            // Actualizar recibo con datos extraídos
            receipt.VendorName = extractedData.VendorName ?? receipt.VendorName;
            receipt.ReceiptDate = extractedData.ReceiptDate ?? receipt.ReceiptDate;
            receipt.TotalAmount = extractedData.TotalAmount ?? receipt.TotalAmount;
            receipt.TaxAmount = extractedData.TaxAmount;
            receipt.ConfidenceScore = (decimal)extractedData.ConfidenceScore;
            receipt.IsAiProcessed = true;
            receipt.ProcessingStatus = "completed";
            receipt.ProcessedAt = DateTime.UtcNow;

            // Buscar o crear vendor
            if (!string.IsNullOrEmpty(extractedData.VendorName))
            {
                var vendor = await GetOrCreateVendorAsync(receipt.CompanyId, extractedData.VendorName);
                receipt.VendorId = vendor.Id;
            }

            // Asignar categoría si fue sugerida
            if (!string.IsNullOrEmpty(extractedData.SuggestedCategory))
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CompanyId == receipt.CompanyId &&
                                             c.Name.ToLower().Contains(extractedData.SuggestedCategory.ToLower()));
                if (category != null)
                    receipt.CategoryId = category.Id;
            }

            await _context.SaveChangesAsync();

            // Log de procesamiento
            var log = new AiProcessingLog
            {
                Id = Guid.NewGuid(),
                ReceiptId = receiptId,
                Provider = "claude",
                Model = "claude-3-sonnet",
                ExtractionSuccessful = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.AiProcessingLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Marcar como fallido
            var receipt = await _context.Receipts.FindAsync(receiptId);
            if (receipt != null)
            {
                receipt.ProcessingStatus = "failed";
                receipt.IsFlagged = true;
                receipt.FlagReason = $"Error en IA: {ex.Message}";
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task<(List<ReceiptResponseDto> Receipts, int TotalCount)> GetUserReceiptsAsync(
        Guid userId, int page, int pageSize, string? status = null, Guid? categoryId = null)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new UnauthorizedAccessException();

        var query = _context.Receipts
            .Include(r => r.Category)
            .Where(r => r.CompanyId == user.CompanyId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        if (categoryId.HasValue)
            query = query.Where(r => r.CategoryId == categoryId);

        var totalCount = await query.CountAsync();

        var receipts = await query
            .OrderByDescending(r => r.ReceiptDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = new List<ReceiptResponseDto>();
        foreach (var receipt in receipts)
        {
            dtos.Add(await MapToDto(receipt));
        }

        return (dtos, totalCount);
    }

    public async Task<ReceiptResponseDto?> GetReceiptByIdAsync(Guid receiptId, Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) throw new UnauthorizedAccessException();

        var receipt = await _context.Receipts
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == receiptId && r.CompanyId == user.CompanyId);

        return receipt == null ? null : await MapToDto(receipt);
    }

    public async Task<ReceiptResponseDto> UpdateReceiptAsync(Guid receiptId, Guid userId, CreateReceiptDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        var receipt = await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == receiptId && r.CompanyId == user!.CompanyId);

        if (receipt == null) throw new KeyNotFoundException("Recibo no encontrado");

        receipt.ReceiptDate = dto.ReceiptDate.HasValue ? DateOnly.FromDateTime(dto.ReceiptDate.Value): receipt.ReceiptDate;
        receipt.TotalAmount = dto.TotalAmount ?? receipt.TotalAmount;
        receipt.VendorName = dto.VendorName ?? receipt.VendorName;
        receipt.CategoryId = dto.CategoryId ?? receipt.CategoryId;
        receipt.Notes = dto.Notes ?? receipt.Notes;
        receipt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapToDto(receipt);
    }

    public async Task<bool> DeleteReceiptAsync(Guid receiptId, Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        var receipt = await _context.Receipts
            .FirstOrDefaultAsync(r => r.Id == receiptId && r.CompanyId == user!.CompanyId);

        if (receipt == null) return false;

        // Eliminar archivo físico
        await _fileStorage.DeleteFileAsync(receipt.ImageUrl);

        _context.Receipts.Remove(receipt);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<Vendor> GetOrCreateVendorAsync(Guid companyId, string vendorName)
    {
        var normalizedName = vendorName.ToUpper().Trim();

        var vendor = await _context.Vendors
            .FirstOrDefaultAsync(v => v.CompanyId == companyId && v.NormalizedName == normalizedName);

        if (vendor == null)
        {
            vendor = new Vendor
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Name = vendorName,
                NormalizedName = normalizedName,
                CreatedAt = DateTime.UtcNow
            };
            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();
        }

        return vendor;
    }

    private async Task<ReceiptResponseDto> MapToDto(Receipt receipt)
    {
        return new ReceiptResponseDto
        {
            Id = receipt.Id,
            ReceiptDate = receipt.ReceiptDate,
            TotalAmount = receipt.TotalAmount,
            Currency = receipt.Currency ?? "USD",
            VendorName = receipt.VendorName,
            CategoryName = receipt.Category?.Name,
            CategoryColor = receipt.Category?.Color,
            Status = receipt.Status,
            ImageUrl = receipt.ImageUrl,
            IsAiProcessed = (bool)receipt.IsAiProcessed,
            ConfidenceScore = receipt.ConfidenceScore,
            Notes = receipt.Notes,
            CreatedAt = (DateTime)receipt.CreatedAt
        };
    }
}

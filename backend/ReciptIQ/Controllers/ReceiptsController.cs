using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReciptIQ.API.Services.Interfaces;
using ReciptIQ.DTOs.Receipts;
using System.Security.Claims;

namespace ReciptIQ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly IReceiptService _receiptService;

    public ReceiptsController(IReceiptService receiptService)
    {
        _receiptService = receiptService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst("userId")?.Value ?? throw new UnauthorizedAccessException());

    [HttpPost("upload")]
    public async Task<IActionResult> UploadReceipt([FromForm] IFormFile file, [FromForm] CreateReceiptDto? dto)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No se proporcionó archivo" });

        var userId = GetUserId();
        var receipt = await _receiptService.CreateReceiptAsync(userId, file, dto);

        return CreatedAtAction(nameof(GetReceipt), new { id = receipt.Id }, receipt);
    }

    [HttpGet]
    public async Task<IActionResult> GetReceipts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] Guid? categoryId = null)
    {
        var userId = GetUserId();
        var (receipts, totalCount) = await _receiptService.GetUserReceiptsAsync(userId, page, pageSize, status, categoryId);

        return Ok(new
        {
            data = receipts,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            }
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReceipt(Guid id)
    {
        var userId = GetUserId();
        var receipt = await _receiptService.GetReceiptByIdAsync(id, userId);

        if (receipt == null)
            return NotFound(new { message = "Recibo no encontrado" });

        return Ok(receipt);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReceipt(Guid id, [FromBody] CreateReceiptDto dto)
    {
        try
        {
            var userId = GetUserId();
            var receipt = await _receiptService.UpdateReceiptAsync(id, userId, dto);
            return Ok(receipt);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReceipt(Guid id)
    {
        var userId = GetUserId();
        var deleted = await _receiptService.DeleteReceiptAsync(id, userId);

        if (!deleted)
            return NotFound(new { message = "Recibo no encontrado" });

        return NoContent();
    }
}

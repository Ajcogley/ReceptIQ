using ReciptIQ.API.Services.Interfaces;
using ReciptIQ.Services.Interfaces;

namespace ReciptIQ.API.Services.Implementations;

public class ClaudeAIService : IClaudeAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ClaudeAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ReceiptExtractionResult> ExtractReceiptDataAsync(string imageUrl)
    {
        // TODO: Implementar llamada real a Claude API
        // Por ahora, devolver datos mock para testing

        await Task.Delay(2000); // Simular procesamiento

        return new ReceiptExtractionResult
        {
            VendorName = "Super 99",
            ReceiptDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            TotalAmount = 45.99m,
            TaxAmount = 3.45m,
            SuggestedCategory = "Alimentos",
            ConfidenceScore = 0.92m,
            Items = new List<ReceiptItemData>
            {
                new() { Description = "Pan", Quantity = 2, UnitPrice = 1.50m, TotalPrice = 3.00m },
                new() { Description = "Leche", Quantity = 1, UnitPrice = 4.99m, TotalPrice = 4.99m }
            }
        };
    }
}

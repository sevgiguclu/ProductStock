namespace ProductStock.Application.DTOs.Stock;

// Stok giriş/çıkış "formu".
// Hangi ürün olduğu ve giriş mi çıkış mı olduğu ADRESTEN gelir:
//   POST api/products/{productId}/stock/in   → giriş
//   POST api/products/{productId}/stock/out  → çıkış
public class StockChangeRequest
{
    // Kaç tane? Her zaman pozitif yazılır (çıkışta da 3 yazılır, −3 değil).
    public int Quantity { get; set; }

    // Neden? Ör. "Tedarikçiden alım", "Satış", "Müşteri iadesi", "Fire"
    public string Reason { get; set; } = string.Empty;
}

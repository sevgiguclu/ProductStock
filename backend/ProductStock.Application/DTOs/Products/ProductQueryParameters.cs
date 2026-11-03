namespace ProductStock.Application.DTOs.Products;

// Ürün listesi için "sipariş fişi": arama, filtre, sıralama ve sayfa tercihleri.
// ASP.NET bu fişi adresin "?" sonrasından (query string) kendisi doldurur:
//   GET api/products?search=klavye&category=Kablolar&minPrice=100&page=2
// Hiçbiri zorunlu değil; boş bırakılan tercih uygulanmaz.
public class ProductQueryParameters
{
    // Adında VEYA SKU'sunda bu metin geçen ürünler (büyük-küçük harf fark etmez).
    public string? Search { get; set; }

    // Sadece bu kategorideki ürünler (birebir eşleşme).
    public string? Category { get; set; }

    // Fiyat aralığı. "decimal?" → boş bırakılabilir.
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Stok durumu filtresi: outOfStock, critical, sufficient
    public StockStatusFilter? StockStatus { get; set; }

    // Sıralama alanı ve yönü. Varsayılan: ada göre A'dan Z'ye.
    public ProductSortBy SortBy { get; set; } = ProductSortBy.Name;
    public bool SortDescending { get; set; }

    // Kaçıncı sayfa (1'den başlar) ve sayfada kaç ürün olacağı.
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

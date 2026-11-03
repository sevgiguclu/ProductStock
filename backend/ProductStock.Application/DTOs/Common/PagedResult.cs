namespace ProductStock.Application.DTOs.Common;

// Sayfalı liste cevabı: o sayfadaki kayıtlar + sayfa bilgileri.
// <T>: "generic" (genel) tip. T yerine ne gelirse onun listesini taşır:
// PagedResult<ProductResponse> = ürünlerin sayfası; ileride PagedResult<StockMovementResponse> da olabilir.
public class PagedResult<T>
{
    // Bu sayfadaki kayıtlar
    public List<T> Items { get; set; } = [];

    public int Page { get; set; }
    public int PageSize { get; set; }

    // Filtreye uyan TÜM kayıtların sayısı (sadece bu sayfadakiler değil)
    public long TotalCount { get; set; }

    // Hesaplanmış alanlar: frontend'deki sayfa düğmeleri için.
    // Örn. 47 kayıt, sayfada 20 → 47/20 = 2.35 → yukarı yuvarla → 3 sayfa
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

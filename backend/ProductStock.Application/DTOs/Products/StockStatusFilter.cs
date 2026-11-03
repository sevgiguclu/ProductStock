namespace ProductStock.Application.DTOs.Products;

// enum: sadece belirli seçenekleri olan tip. Adreste ?stockStatus=critical gibi yazılır
// (büyük-küçük harf fark etmez). Listede olmayan bir değer gelirse ASP.NET kabul etmez.
public enum StockStatusFilter
{
    // Stoğu tamamen bitmiş: stok = 0
    OutOfStock,

    // Kritik seviyede: stok <= kritik seviye (stoğu bitmiş olanlar da dahil). "Kritik stok uyarısı" listesi.
    Critical,

    // Yeterli: stok > kritik seviye
    Sufficient
}

using ProductStock.Application.DTOs.Products;
using ProductStock.Domain.Entities;

namespace ProductStock.Application.Interfaces;

// Ürünlerin veritabanında saklanması için "sözleşme" (interface).
// Burada sadece NELERİN yapılabileceği yazılır, NASIL yapılacağı yazılmaz.
// Asıl kod Infrastructure katmanında, MongoDB kullanılarak yazılacak (ProductRepository).
//
// Task: metodun "beklenebilir" (async) olduğunu gösterir. Veritabanı uzakta olduğu için
// cevap gelene kadar uygulama başka işlerle uğraşabilsin diye async kullanıyoruz.
//
// CancellationToken: "bu işi artık bırak" sinyali. Örneğin kullanıcı sayfayı kapatırsa
// veritabanı sorgusunu boşuna sürdürmeyiz.
public interface IProductRepository
{
    // Arama/filtre/sıralama uygulayıp istenen sayfadaki ürünleri getirir.
    // İki bilgi birlikte döner (tuple): o sayfadaki ürünler + filtreye uyan TOPLAM ürün sayısı.
    Task<(List<Product> Items, long TotalCount)> SearchAsync(ProductQueryParameters query, CancellationToken cancellationToken = default);

    // Id'si verilen ürünü getirir; bulunamazsa null döner ("?" işareti null olabileceğini gösterir).
    Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    // Aynı SKU (stok kodu) ile başka bir ürün var mı? Aynı kodla iki ürün olmasın diye kullanılacak.
    Task<bool> SkuExistsAsync(string sku, CancellationToken cancellationToken = default);

    // Yeni ürünü kaydeder. Kayıttan sonra product.Id veritabanının verdiği Id ile dolar.
    Task CreateAsync(Product product, CancellationToken cancellationToken = default);

    // Ürünün bilgilerini (ad, SKU, açıklama, kategori, fiyat, kritik seviye) günceller.
    // Stok miktarına DOKUNMAZ: stok sadece stok giriş/çıkış işlemleriyle değişir.
    // Ürün bulunamadıysa false döner.
    Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default);

    // Ürünü siler; ürün bulunamadıysa false döner.
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

namespace ProductStock.Domain.Enums;

// Kullanıcı rolleri. Token'ın (bilekliğin) üzerine yazılır; adresler buna göre izin verir.
public enum UserRole
{
    // Yönetici: her şey (ürün ekle/düzenle/sil, kullanıcı yönetimi, stok işlemleri)
    Admin,

    // Depo görevlisi: ürünleri görme, stok giriş/çıkış, fiş geçmişi
    WarehouseStaff
}

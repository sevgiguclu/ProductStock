namespace ProductStock.Application.DTOs.Auth;

// Kapı görevlisinin her istekte ihtiyaç duyduğu İKİ bilgi: kullanıcı aktif mi, güvenlik damgası ne?
// Kullanıcının tüm kaydını (şifre kıyması dahil) getirmek yerine veritabanından sadece bu iki alan istenir (projection).
//
// record: sadece veri taşımak için kısa yazımlı sınıf. Bu tek satır; iki özellik, constructor ve
// karşılaştırma (iki record'un değerleri aynıysa eşittir) kodlarını otomatik üretir.
// SecurityStamp "string?": damgası henüz olmayan eski kullanıcılarda alan veritabanında yok → null gelir.
public sealed record UserAuthState(bool IsActive, string? SecurityStamp);

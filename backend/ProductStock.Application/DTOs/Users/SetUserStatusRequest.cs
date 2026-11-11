namespace ProductStock.Application.DTOs.Users;

// Kullanıcıyı aktif/pasif yapma formu: { "isActive": false }
public class SetUserStatusRequest
{
    // bool? ("boş olabilir bool"): Düz bool olsaydı, gövdede alan unutulduğunda varsayılan değer
    // false olurdu ve kullanıcı YANLIŞLIKLA pasif yapılırdı. Boş gelirse servis 400 verir.
    public bool? IsActive { get; set; }
}

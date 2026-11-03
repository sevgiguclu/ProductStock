namespace ProductStock.Infrastructure.Persistence;

// MongoDB koleksiyon ("klasör") adları tek yerde. Aynı adı farklı dosyalarda elle yazarsak
// bir yazım hatası index'i ya da veriyi yanlış koleksiyona gönderebilir.
public static class MongoCollectionNames
{
    public const string Products = "products";
}

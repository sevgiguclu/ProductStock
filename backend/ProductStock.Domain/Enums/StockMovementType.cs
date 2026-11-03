namespace ProductStock.Domain.Enums;

// Stok hareketinin (fişin) türü.
public enum StockMovementType
{
    // Giriş: stok artar (ör. tedarikçiden alım, müşteri iadesi)
    In,

    // Çıkış: stok azalır (ör. satış, fire, kayıp)
    Out
}

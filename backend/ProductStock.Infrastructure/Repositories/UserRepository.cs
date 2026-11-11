using MongoDB.Bson;
using MongoDB.Driver;
using ProductStock.Application.Exceptions;
using ProductStock.Application.Interfaces;
using ProductStock.Domain.Entities;
using ProductStock.Infrastructure.Persistence;

namespace ProductStock.Infrastructure.Repositories;

// Kullanıcıları MongoDB'de saklayan eleman.
public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbContext context)
    {
        _users = context.Database.GetCollection<User>(MongoCollectionNames.Users);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _users.Find(u => u.Username == username).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        // Id ObjectId biçiminde değilse veritabanına hiç sormadan "yok" (ProductRepository'deki gibi).
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _users
            .Find(FilterDefinition<User>.Empty)   // Empty: filtre yok, hepsi
            .SortBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        var count = await _users.CountDocumentsAsync(FilterDefinition<User>.Empty, new CountOptions { Limit = 1 }, cancellationToken);
        return count > 0;
    }

    public Task<bool> SetActiveAsync(string id, bool isActive, CancellationToken cancellationToken = default)
        => SetFieldAsync(id, Builders<User>.Update.Set(u => u.IsActive, isActive), cancellationToken);

    public Task<bool> UpdatePasswordHashAsync(string id, string passwordHash, CancellationToken cancellationToken = default)
        => SetFieldAsync(id, Builders<User>.Update.Set(u => u.PasswordHash, passwordHash), cancellationToken);

    // Ortak iş: belgenin tamamını değil, SADECE verilen alanı değiştir ($set).
    // Böylece aynı anda başka bir alan değişse bile onu ezmeyiz (ürün güncellemedeki mantık).
    private async Task<bool> SetFieldAsync(string id, UpdateDefinition<User> update, CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        var result = await _users.UpdateOneAsync(u => u.Id == id, update, cancellationToken: cancellationToken);

        // MatchedCount: filtreye uyan belge sayısı. 0 ise kullanıcı yok.
        // (ModifiedCount'a bakmıyoruz: zaten pasif olanı tekrar pasif yapmak "değişmedi" der ama hata değildir.)
        return result.MatchedCount > 0;
    }

    public async Task CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            await _users.InsertOneAsync(user, cancellationToken: cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Kullanıcı adı benzersiz index'i ikinci kaydı reddetti.
            throw new ConflictException($"'{user.Username}' kullanıcı adı zaten kullanılıyor.");
        }
    }
}

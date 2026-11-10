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

using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public class EfUsersStorage(SanitizerDbContext db) : IUsersStorage
{
    public async Task<UserEntity?> GetAsync(string login)
        => await db.Users.FirstOrDefaultAsync(x => x.Login == login);

    public async Task CreateOrUpdateAsync(UserEntity entity)
    {
        var existing = await GetAsync(entity.Login);
        if (existing is null)
        {
            db.Users.Add(entity);
        }
        else
        {
            existing.Login = entity.Login;
            existing.Password = entity.Password;
            existing.ApiKeyId = entity.ApiKeyId;
        }

        await db.SaveChangesAsync();
    }
}
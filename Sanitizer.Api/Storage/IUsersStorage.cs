using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public interface IUsersStorage
{
    public Task<UserEntity?> GetAsync(string login);
    public Task CreateOrUpdateAsync(UserEntity entity);
}
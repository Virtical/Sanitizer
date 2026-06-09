using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Sanitizer.Api.Controllers.Internal.Client.Requests;
using Sanitizer.Api.Storage;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Services;

public class UsersService(IUsersStorage usersStorage, ApiKeyService apiKeyService)
{
    public async Task<Result<string>> RegisterAsync(string login, string password)
    {
        var user = await usersStorage.GetAsync(login);
        if (user is not null)
        {
            return Result.Fail<string>($"User with login {login} already exists.");
        }
        
        var token = await apiKeyService.CreateAsync(Guid.NewGuid().ToString(), DateTime.Now.AddHours(1));
        var newUser = new UserEntity
        {
            Login = login,
            Password = Hash(password),
            ApiKeyId = token.Id
        };
        
        await usersStorage.CreateOrUpdateAsync(newUser);

        
        return Result.Ok(token.KeyHash);
    }
    
    public async Task<Result<string>> LoginAsync(LoginRequest loginRequest)
    {
        var user = await usersStorage.GetAsync(loginRequest.Login);
        if (user is null)
        {
            return Result.Fail<string>($"User with login {loginRequest.Login} not found");
        }
        
        if (user.Password != Hash(loginRequest.Password))
        {
            return Result.Fail<string>("Wrong password");
        }

        var token = await apiKeyService.GetTokenAsync(user.ApiKeyId);
        if (!token.IsValid())
        {
            var newToken = await apiKeyService.CreateAsync(Guid.NewGuid().ToString(), DateTime.Now.AddHours(1));
            user.ApiKeyId = newToken.Id;
            await usersStorage.CreateOrUpdateAsync(user);
            return Result.Ok(newToken.KeyHash);
        }
        
        return Result.Ok(token.KeyHash);
    }

    private static string Hash(string password) =>
        Convert
            .ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)))
            .ToLowerInvariant();
}
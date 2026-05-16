using Sanitizer.Api.Controllers.Client.Requests;
using Sanitizer.Api.Models;
using Sanitizer.Api.Storage.Data.Entities;

namespace Sanitizer.Api.Storage;

public interface IProfileStorage
{
    Task<SanitizationProfileEntity[]> GetAllAsync();
    Task<SanitizationProfileEntity?> GetByIdAsync(string id);
    Task SaveAsync(SanitizationProfileEntity profileRequest);
    Task<bool> DeleteAsync(string id);
}

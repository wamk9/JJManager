using JJManager.Core.Interfaces.Repositories;
using JJManager.Core.Profile;
using JJManager.Core.Interfaces.Services;

namespace JJManager.Infrastructure.Services;

/// <summary>
/// Service for database operations using Repository pattern
/// Acts as a facade to simplify access to repositories
/// </summary>
public class AppConfigService : IAppConfigService
{
   private readonly IUnitOfWork _unitOfWork;

    public AppConfigService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _unitOfWork.InitializeAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao inicializar o banco de dados", ex);
        }
    }

    public async Task<string?> GetConfigAsync(string key)
    {
        return await _unitOfWork.Configs.GetValueAsync(key);
    }

    public async Task SetConfigAsync(string key, string value)
    {
        await _unitOfWork.Configs.SetValueAsync(key, value);
    }

    public string GetDatabaseFolder()
    {
        return _unitOfWork.GetDatabaseFolder(); ;
    }
}

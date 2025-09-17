using Cysharp.Threading.Tasks;

namespace CityBuilder.ContractInterfaces
{
    public interface ISaveLoadService
    {
        UniTask SaveAsync(CityBuilder.Domain.Models.SaveData data);
        UniTask<CityBuilder.Domain.Models.SaveData> LoadAsync();
    }
}

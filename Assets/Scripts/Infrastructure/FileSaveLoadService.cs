
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using CityBuilder.ContractInterfaces;
using CityBuilder.Domain.Models;

namespace CityBuilder.Infrastructure
{
    public class FileSaveLoadService : ISaveLoadService
    {
        private readonly string _savePath;
        private readonly JsonSerializerSettings _jsonSettings;

        public FileSaveLoadService()
        {
            this._savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "savedata.json");
            this._jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
        }

        public async UniTask SaveAsync(SaveData data)
        {
            string json = JsonConvert.SerializeObject(data, this._jsonSettings);
            await File.WriteAllTextAsync(this._savePath, json);
            Debug.Log($"Game saved to: {this._savePath}");
        }

        public async UniTask<SaveData> LoadAsync()
        {
            if (!File.Exists(this._savePath))
            {
                Debug.LogWarning("Save file not found.");
                return null;
            }

            string json = await File.ReadAllTextAsync(this._savePath);
            SaveData data = JsonConvert.DeserializeObject<SaveData>(json, this._jsonSettings);
            Debug.Log("Game loaded.");
            return data;
        }
    }
}
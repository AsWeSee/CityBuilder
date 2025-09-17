using System;
using System.Threading;
using VContainer.Unity;
using MessagePipe;
using Cysharp.Threading.Tasks;
using CityBuilder.Domain.MessageDTO;

public class AutoSaveService : IInitializable, IDisposable
{
    private const float AUTOSAVE_INTERVAL_SECONDS = 60.0f;
    
    private readonly IPublisher<SaveGameRequestDTO> _saveRequestPublisher;
    private readonly CancellationTokenSource _cancellation = new();

    public AutoSaveService(IPublisher<SaveGameRequestDTO> saveRequestPublisher)
    {
        this._saveRequestPublisher = saveRequestPublisher;
    }

    public void Initialize()
    {
        this.AutoSaveLoop(this._cancellation.Token).Forget();
    }

    private async UniTaskVoid AutoSaveLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(AUTOSAVE_INTERVAL_SECONDS), cancellationToken: token);
            
            // Мы не сохраняем напрямую, а просто просим систему сохраниться.
            // Это позволяет повторно использовать логику SaveGameUseCase.
            this._saveRequestPublisher.Publish(new SaveGameRequestDTO());
        }
    }

    public void Dispose() => this._cancellation.Cancel();
}
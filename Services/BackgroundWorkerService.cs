using System.Threading.Channels;

namespace Telegram.Bot.Services
{
    /// <summary>
    /// Implementação do worker de background que executa tarefas em thread separada
    /// </summary>
    public class BackgroundWorkerService : IBackgroundWorker
    {
        private readonly Channel<Func<Task>> _taskChannel = Channel.CreateUnbounded<Func<Task>>();
        private CancellationTokenSource? _cts;
        private Task? _processingTask;
        private bool _isRunning;

        public bool IsRunning => _isRunning;

        public void Start()
        {
            if (_isRunning) return;

            _cts = new CancellationTokenSource();
            _isRunning = true;

            // Inicia thread de processamento em background
            _processingTask = ProcessTasksAsync(_cts.Token);

        }

        public async Task EnqueueAsync(Func<Task> task)
        {
            if (!_isRunning)
            {
                // Se não estiver rodando, apenas executa na thread atual
                await task();
                return;
            }

            try
            {
                await _taskChannel.Writer.WriteAsync(task);
            }
            catch (ChannelClosedException ex)
            {
                await task(); // Fallback: executa na thread atual
            }
        }

        public async Task StopAsync()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _taskChannel.Writer.Complete();

            if (_processingTask != null)
            {
                try
                {
                    await _processingTask;
                }
                catch (OperationCanceledException)
                {
                    // Esperado ao cancelar
                }
            }

            _cts?.Cancel();
            _cts?.Dispose();

            System.Diagnostics.Debug.WriteLine("[BackgroundWorker] Parado");
        }

        private async Task ProcessTasksAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var task in _taskChannel.Reader.ReadAllAsync(cancellationToken))
                {
                    try
                    {
                        await task();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}

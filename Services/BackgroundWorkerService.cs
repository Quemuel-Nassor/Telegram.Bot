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

            System.Diagnostics.Debug.WriteLine("[BackgroundWorker] Iniciado");
        }

        public async Task EnqueueAsync(Func<Task> task)
        {
            if (!_isRunning)
            {
                System.Diagnostics.Debug.WriteLine("[BackgroundWorker] Não está rodando, enfileirando fallará");
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
                System.Diagnostics.Debug.WriteLine($"[BackgroundWorker] Canal fechado: {ex.Message}");
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
                        System.Diagnostics.Debug.WriteLine("[BackgroundWorker] Processando tarefa...");
                        await task();
                        System.Diagnostics.Debug.WriteLine("[BackgroundWorker] Tarefa concluída");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[BackgroundWorker] Erro ao processar tarefa: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("[BackgroundWorker] Processamento cancelado");
            }
        }
    }
}

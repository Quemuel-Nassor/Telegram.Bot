namespace Telegram.Bot.Services
{
    /// <summary>
    /// Serviço que executa tarefas pesadas em thread de background,
    /// prevenindo congelamento da UI thread
    /// </summary>
    public interface IBackgroundWorker
    {
        /// <summary>
        /// Enfileira uma tarefa para executar em thread de background
        /// </summary>
        /// <param name="task">Tarefa a executar</param>
        Task EnqueueAsync(Func<Task> task);

        /// <summary>
        /// Inicia o processamento de tarefas em background
        /// </summary>
        void Start();

        /// <summary>
        /// Para o processamento gracefully
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// Indica se o worker está ativo
        /// </summary>
        bool IsRunning { get; }
    }
}

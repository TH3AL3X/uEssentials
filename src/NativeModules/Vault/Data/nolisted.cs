namespace Essentials.NativeModules.Vault.data
{
    using Essentials.Core;
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;


    internal class SerialQueue
    {
        private readonly ConcurrentQueue<Func<Task>> _queue = new();
        private bool _isProcessing = false;

        public void Enqueue(Func<Task> action)
        {
            _queue.Enqueue(action);
            ProcessQueue();
        }

        private async void ProcessQueue()
        {
            if (_isProcessing) return;
            _isProcessing = true;
            while (_queue.TryDequeue(out var action))
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    EssCore.print($"SerialQueueex: {ex}");
                }
            }
            _isProcessing = false;
        }
    }
}

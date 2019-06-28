using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bursa
{
    public class AsyncProcessor<TTaskProvider, TTask>
    {
        public event Func<TTaskProvider, TTask, Task> TaskComplete;
        public Func<object, Task<TTask>> TaskRetriever;

        public List<Task<TTask>> Tasks = new List<Task<TTask>>();
        public List<TTaskProvider> Providers = new List<TTaskProvider>();

        public Dictionary<Task<TTask>, TTaskProvider> ProviderByTask = new Dictionary<Task<TTask>, TTaskProvider>();
        public Dictionary<TTaskProvider, Task<TTask>> TaskByProvider = new Dictionary<TTaskProvider, Task<TTask>>();
        
        private Task<TTask> LoopCanceller;
        private ManualResetEvent CancelFlag = new ManualResetEvent(false);

        public AsyncProcessor()
        {
        }

        public void Add(TTaskProvider provider)
        {
            lock (Providers)
            {
                Providers.Add(provider);
            }

            CancelFlag.Set();
        }

        public void Remove(TTaskProvider provider)
        {
            lock (Providers)
            {
                Providers.Remove(provider);

                if (TaskByProvider.ContainsKey(provider))
                {
                    var task = TaskByProvider[provider];

                    TaskByProvider.Remove(provider);
                    ProviderByTask.Remove(task);
                    Tasks.Remove(task);
                }
            }
        }

        public async Task Process()
        {
            if(!Tasks.Any())
            {
                lock (Providers)
                {
                    foreach (var provider in Providers.ToList())
                    {
                        var new_task = TaskRetriever(provider);
                        TaskByProvider[provider] = new_task;
                        ProviderByTask[new_task] = provider;
                        Tasks.Add(new_task);
                    }
                }
            }

            if (!Tasks.Any())
            {
                await Task.Delay(500);
                return;
            }

            var finished_task = await Task.WhenAny(Tasks);

            if (finished_task == LoopCanceller)
            {
                CancelFlag.Reset();
                LoopCanceller = new Task<TTask>(() => { CancelFlag.WaitOne(); return default; });
            }
            else if (finished_task.IsCompletedSuccessfully)
            {
                if(ProviderByTask.ContainsKey(finished_task)) // this provider might have been removed, check for that
                    TaskComplete?.Invoke(ProviderByTask[finished_task], finished_task.Result);
            }

            Tasks.Remove(finished_task);
            ProviderByTask.Remove(finished_task);
            finished_task.Dispose();

            lock (Providers)
            {
                foreach (var provider in Providers)
                {
                    if (TaskByProvider.ContainsKey(provider))
                        continue;

                    var new_task = TaskRetriever(provider);
                    TaskByProvider[provider] = new_task;
                    ProviderByTask[new_task] = provider;
                    Tasks.Add(new_task);
                }
            }
        }
    }
}

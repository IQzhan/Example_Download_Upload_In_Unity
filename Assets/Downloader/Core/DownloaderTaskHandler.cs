using System.Collections.Concurrent;
using System.Collections.Generic;

namespace E.Net
{
    public abstract class DownloaderTaskHandler
    {
        protected interface IDownloaderTask
        {
            void RunTask(System.Action action);
            bool IsEnded();
        }

        protected abstract IDownloaderTask GetTaskInstance();

        private int maxTaskNum = 10;

        public int MaxTaskNum 
        {
            get { return maxTaskNum; }
            set { if (value < 0) value = 0; maxTaskNum = value; }
        }

        private readonly ConcurrentQueue<System.Action> actionQueue = new ConcurrentQueue<System.Action>();

        private readonly LinkedList<IDownloaderTask> taskList = new LinkedList<IDownloaderTask>();

        public void AddTask(ref System.Action action)
        {
            actionQueue.Enqueue(action);
        }

        public void Tick()
        {
            TryEndTask();
            TryRunTask();
        }

        private void TryEndTask()
        {
            LinkedListNode<IDownloaderTask> node = taskList.First;
            while(node != null)
            {
                LinkedListNode<IDownloaderTask> next = node.Next;
                if (node.Value.IsEnded())
                {
                    taskList.Remove(node);
                }
                node = next;
            }
        }

        private void TryRunTask()
        {
            while (taskList.Count < MaxTaskNum && actionQueue.TryDequeue(out System.Action action))
            {
                IDownloaderTask downloaderTask = GetTaskInstance();
                taskList.AddLast(downloaderTask);
                downloaderTask.RunTask(action);
            }
        }
    }
}

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace E.Data
{
    public abstract class TaskHandler : System.IDisposable
    {
        protected abstract class ITask
        {
            public System.Action bodyAction;
            public System.Action cleanAction;
            //TODO
            public System.Action<bool> isClosed;
            public abstract void RunTask();
            public abstract void RunClear();
            public abstract bool IsEnded();
        }

        protected abstract ITask GetTaskInstance();

        private int maxTaskNum = 10;

        public int MaxTaskNum
        {
            get { return maxTaskNum; }
            set { if (value < 0) value = 0; maxTaskNum = value; }
        }

        private ConcurrentQueue<ITask> actionQueue = new ConcurrentQueue<ITask>();

        private LinkedList<ITask> taskList = new LinkedList<ITask>();

        public void AddTask(in System.Action body, in System.Action clean)
        {
            if(body != null && clean != null)
            {
                ITask task = GetTaskInstance();
                task.bodyAction = body;
                task.cleanAction = clean;
                actionQueue.Enqueue(task);
            }
        }

        public void Tick()
        {
            if (!disposedValue)
            {
                TryEndTask();
                TryRunTask();
            }
        }

        private void TryEndTask()
        {
            LinkedListNode<ITask> node = taskList.First;
            while (node != null)
            {
                LinkedListNode<ITask> next = node.Next;
                if (node.Value.IsEnded())
                {
                    node.Value.RunClear();
                    taskList.Remove(node);
                }
                node = next;
            }
        }

        private void TryRunTask()
        {
            while (taskList.Count < MaxTaskNum && actionQueue.TryDequeue(out ITask task))
            {
                taskList.AddLast(task);
                task.RunTask();
            }
        }

        private bool disposedValue;

        private void EndTasks()
        {
            LinkedListNode<ITask> node = taskList.First;
            while (node != null)
            {
                LinkedListNode<ITask> next = node.Next;
                node.Value.RunClear();
                taskList.Remove(node);
                node = next;
            }
            taskList = null;
        }

        private void ClearQueue()
        {
            while (!actionQueue.IsEmpty && actionQueue.TryDequeue(out ITask task))
            { task.RunClear(); }
            actionQueue = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                { ClearQueue(); }
                EndTasks();
                disposedValue = true;
            }
        }

        ~TaskHandler()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}

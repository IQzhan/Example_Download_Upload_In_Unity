using System.Collections.Concurrent;
using System.Collections.Generic;

namespace E.Data
{
    public abstract class TaskHandler
    {
        protected interface ITask
        {
            void RunTask(System.Action action);
            bool IsEnded();
        }

        protected abstract ITask GetTaskInstance();

        private int maxTaskNum = 10;

        public int MaxTaskNum
        {
            get { return maxTaskNum; }
            set { if (value < 0) value = 0; maxTaskNum = value; }
        }

        private readonly ConcurrentQueue<System.Action> actionQueue = new ConcurrentQueue<System.Action>();

        private readonly LinkedList<ITask> taskList = new LinkedList<ITask>();

        public void AddTask(in System.Action action)
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
            LinkedListNode<ITask> node = taskList.First;
            while (node != null)
            {
                LinkedListNode<ITask> next = node.Next;
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
                ITask downloaderTask = GetTaskInstance();
                taskList.AddLast(downloaderTask);
                downloaderTask.RunTask(action);
            }
        }
    }
}

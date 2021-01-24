using System.Threading.Tasks;

namespace E.Data
{
    public class StandloneTaskHandler : TaskHandler
    {
        protected StandloneTaskHandler() { }

        private class ClonerTask : ITask
        {
            public bool IsEnded()
            {
                return task.IsCompleted || task.IsCanceled || task.IsFaulted;
            }

            private Task task;

            public void RunTask(System.Action action)
            {
                task = Task.Run(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (System.Exception e)
                    {
                        DataProcessorDebug.LogException(e);
                    }
                });
            }
        }

        protected override ITask GetTaskInstance()
        {
            return new ClonerTask();
        }
    }
}

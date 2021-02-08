using System.Threading.Tasks;

namespace E.Data
{
    public class StandloneTaskHandler : TaskHandler
    {
        protected StandloneTaskHandler() { }

        private class DataTask : ITask
        {
            public override bool IsEnded()
            {
                return asyncOperation.IsClosed || (task != null && (task.IsCompleted || task.IsCanceled || task.IsFaulted));
            }

            private Task task;

            public override void RunTask()
            {
                task = Task.Run(() =>
                {
                    try
                    { bodyAction(); }
                    catch (System.Exception e)
                    { DataProcessorDebug.LogException(e); }
                });
            }

            public override void RunClear()
            {
                try
                {
                    cleanAction();
                    task?.Dispose();
                    task = null;
                }
                catch(System.Exception e)
                { DataProcessorDebug.LogException(e); }
            }
        }

        protected override ITask GetTaskInstance()
        { return new DataTask(); }
    }
}

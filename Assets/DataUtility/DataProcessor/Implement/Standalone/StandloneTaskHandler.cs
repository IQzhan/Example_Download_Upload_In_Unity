using System.Threading;
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
            
            private CancellationTokenSource cancelSource;
            
            public override void RunTask()
            {
                cancelSource = new CancellationTokenSource();
                task = Task.Run(() =>
                {
                    try
                    { bodyAction(); }
                    catch (System.Exception e)
                    { DataProcessorDebug.LogException(e); }
                }, cancelSource.Token);
            }

            public override void RunClear()
            {
                try
                {
                    
                    cancelSource?.Cancel();
                    task?.Dispose();
                    
                    cancelSource?.Dispose();
                    task = null;
                    cancelSource = null;
                    cleanAction();
                }
                catch(System.Exception e)
                { DataProcessorDebug.LogException(e); }
            }
        }

        protected override ITask GetTaskInstance()
        { return new DataTask(); }
    }
}

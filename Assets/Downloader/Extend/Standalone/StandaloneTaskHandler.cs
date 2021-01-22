using System.Threading.Tasks;

namespace E.Net
{
    public class StandaloneTaskHandler : DownloaderTaskHandler
    {
        private struct DownloaderTask : IDownloaderTask
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
                        DownloaderDebug.LogError(e.Message + System.Environment.NewLine + e.StackTrace);
                    }
                });
            }
        }

        protected override IDownloaderTask GetTaskInstance()
        {
            return new DownloaderTask();
        }
    }
}

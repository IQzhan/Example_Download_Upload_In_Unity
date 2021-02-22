﻿using System.Threading.Tasks;

namespace E.Data
{
    public class StandloneTaskHandler : TaskHandler
    {
        protected StandloneTaskHandler() { }

        private class DataTask : ITask
        {
            public override bool IsEnded()
            { return task != null && (task.IsCompleted || task.IsCanceled || task.IsFaulted); }

            private Task task;

            public override void RunTask()
            {
                task = Task.Run(() =>
                {
                    try { bodyAction(); }
                    catch (System.Exception e)
                    { DataProcessorDebug.LogException(e); }
                    finally
                    {
                        try { cleanAction(); }
                        catch (System.Exception e)
                        { DataProcessorDebug.LogException(e); }
                    }
                });
            }

            public override void RunClear() { }
        }

        protected override ITask GetTaskInstance()
        { return new DataTask(); }
    }
}
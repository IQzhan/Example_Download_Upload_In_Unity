using System.Collections.Generic;

namespace E.Data
{
    public partial class DataProcessor
    {
        private AsyncOperationGroupImplement asyncOperationGroup;

        public AsyncOperationGroup StartAsyncOperationGroup()
        {
            AsyncOperationGroupImplement asyncOperation = asyncOperationGroup = new AsyncOperationGroupImplement { IsWorking = true };
            commandHandler.AddCommand(() =>
            { if (asyncOperation != null) { asyncOperation.Close(); asyncOperation.onClose?.Invoke(); } },
            () => 
            {
                if(asyncOperation != null)
                {
                    asyncOperation.progress = asyncOperation.GetProgress();
                    asyncOperation.TotalTasks = asyncOperation.list.Count;
                    asyncOperation.SuccessfulTasks = asyncOperation.GetSuccessfulTasks();
                    asyncOperation.FaildTasks = asyncOperation.GetFaildTasks();
                    return (asyncOperation.TotalTasks > 0)
                    && (asyncOperation.CompletedTasks == asyncOperation.TotalTasks);
                }
                return false;
            });
            return asyncOperationGroup;
        }

        public void EndAsyncOperationGroup()
        { asyncOperationGroup = null; }

        private void TryAddAsyncOperation(in AsyncOperation asyncOperation)
        {
            if(asyncOperationGroup != null) { asyncOperationGroup.AddAsyncOperation(asyncOperation); }
        }

        private class AsyncOperationGroupImplement : AsyncOperationGroup 
        {
            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public new int TotalTasks { get { return base.TotalTasks; } set { base.TotalTasks = value; } }

            public new int SuccessfulTasks { get { return base.SuccessfulTasks; } set { base.SuccessfulTasks = value; } }

            public new int FaildTasks { get { return base.FaildTasks; } set { base.FaildTasks = value; } }

            public double progress;

            public override double Progress => progress;

            public int GetSuccessfulTasks()
            {
                if(list != null)
                {
                    int count = list.Count;
                    int successCount = 0;
                    for (int i = 0; i < count; i++)
                    { successCount += list[i].IsProcessingComplete ? 1 : 0; }
                    return successCount;
                }
                return 0;
            }

            public int GetFaildTasks()
            {
                if(list != null)
                {
                    int count = list.Count;
                    int faildCount = 0;
                    for (int i = 0; i < count; i++)
                    { faildCount += list[i].IsError ? 1 : 0; }
                    if (faildCount > 0) IsError = true;
                    return faildCount;
                }
                return 0;
            }

            public double GetProgress()
            {
                if(list != null)
                {
                    int count = list.Count;
                    double total = 0;
                    for(int i = 0; i < count; i++)
                    { total += list[i].Progress; }
                    return total / count;
                }
                return 0;
            }

            public List<AsyncOperation> list = new List<AsyncOperation>();

            public void AddAsyncOperation(AsyncOperation asyncOperation)
            { list?.Add(asyncOperation); }

            public override void Close()
            {
                base.Close();
                list.Clear();
                list = null;
            }
        }
    }
}
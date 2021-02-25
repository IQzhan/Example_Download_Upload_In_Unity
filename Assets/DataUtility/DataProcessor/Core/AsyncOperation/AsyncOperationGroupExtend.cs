using System.Collections.Generic;

namespace E.Data
{
    public partial class DataProcessor
    {
        private AsyncOperationGroupImplement asyncOperationGroup;

        public AsyncOperationGroup StartAsyncOperationGroup()
        {
            asyncOperationGroup = new AsyncOperationGroupImplement
            {
                IsWorking = true
            };
            return asyncOperationGroup;
        }

        public void EndAsyncOperationGroup()
        {
            asyncOperationGroup.Close();
            asyncOperationGroup = null;
        }

        private void TryAddAsyncOperation(AsyncOperation asyncOperation)
        {
            if(asyncOperationGroup != null)
            { asyncOperationGroup.AddAsyncOperation(asyncOperation); }
        }

        private class AsyncOperationGroupImplement : AsyncOperationGroup 
        {
            public new bool IsWorking { get { return base.IsWorking; } set { base.IsWorking = value; } }

            public override int TotalTasks => list == null ? 0 : list.Count;

            public override int SuccessfulTasks => GetSuccessfulTasks();

            public override int FaildTasks => GetFaildTasks();

            public override double Progress => GetProgress();

            private int GetSuccessfulTasks()
            {
                if(list != null)
                {
                    int count = list.Count;
                    int successCount = 0;
                    for (int i = 0; i < count; i++)
                    {
                        successCount += list[i].IsProcessingComplete ? 1 : 0;
                    }
                }
                return 0;
            }

            private int GetFaildTasks()
            {
                if(list != null)
                {
                    int count = list.Count;
                    int faildCount = 0;
                    for (int i = 0; i < count; i++)
                    {
                        faildCount += list[i].IsError ? 1 : 0;
                    }
                    if (faildCount > 0) IsError = true;
                }
                return 0;
            }

            private double GetProgress()
            {
                if(list != null)
                {
                    int count = list.Count;
                    double total = 0;
                    for(int i = 0; i < count; i++)
                    {
                        total += list[i].Progress;
                    }
                    return total / count;
                }
                return 0;
            }

            private List<AsyncOperation> list = new List<AsyncOperation>();

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
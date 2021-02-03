namespace E.Data
{
    public class AsyncOperationGroup : AsyncOperation
    {
        protected AsyncOperationGroup() { }

        public long Total { get; protected set; }

        public long Processed { get; protected set; }

        public override double Progress => (double)Processed / Total;
    }
}

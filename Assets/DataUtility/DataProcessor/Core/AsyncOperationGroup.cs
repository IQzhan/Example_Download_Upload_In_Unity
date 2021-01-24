namespace E.Data
{
    public class AsyncOperationGroup : AsyncOperation
    {
        protected AsyncOperationGroup() { }

        //TODO
        public int Total { get; protected set; }

        public int Succeed { get; protected set; }

        public override double Progress => (double)Succeed / Total;
    }
}

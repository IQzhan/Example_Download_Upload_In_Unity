namespace E.Data
{
    public class DeleteAsyncOperation : AsyncOperation
    {
        protected DeleteAsyncOperation() { }

        protected double progress;

        public override double Progress => progress;
    }
}

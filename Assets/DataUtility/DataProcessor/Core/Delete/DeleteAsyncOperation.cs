namespace E.Data
{
    public class DeleteAsyncOperation : ConnectionAsyncOperation
    {
        protected DeleteAsyncOperation() { }

        protected double progress;

        public override double Progress => progress;
    }
}

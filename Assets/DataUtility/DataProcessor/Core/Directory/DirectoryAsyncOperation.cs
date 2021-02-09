namespace E.Data
{
    public class DirectoryAsyncOperation : ConnectionAsyncOperation
    {
        protected DirectoryAsyncOperation() { }

        private double progress;
        
        public override double Progress => progress;
    }
}

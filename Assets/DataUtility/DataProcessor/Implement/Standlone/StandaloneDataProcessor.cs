namespace E.Data
{
    public class StandaloneDataProcessor : DataProcessor
    {
        public StandaloneDataProcessor(System.Uri cacheUri) : base(cacheUri, new StandloneTaskHandlerInstance(), new StandaloneStreamFactoryInstance()) { }
        
        private class StandloneTaskHandlerInstance : StandloneTaskHandler { }

        private class StandaloneStreamFactoryInstance : StandaloneStreamFactory { }
    }
}

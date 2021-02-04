namespace E.Data
{
    public class StandaloneDataProcessor : DataProcessor
    {
        public StandaloneDataProcessor() : 
            base(new StandloneTaskHandlerInstance(), new StandaloneStreamFactoryInstance()) { }
        
        private class StandloneTaskHandlerInstance : StandloneTaskHandler { }

        private class StandaloneStreamFactoryInstance : StandaloneStreamFactory { }
    }
}

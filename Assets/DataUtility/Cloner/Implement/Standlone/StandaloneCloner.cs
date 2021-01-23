namespace E.Data
{
    public class StandaloneCloner : Cloner
    {
        public StandaloneCloner(System.Uri cacheUri) : base(cacheUri, new StandloneTaskHandlerInstance(), new StandaloneStreamFactoryInstance()) { }
        
        private class StandloneTaskHandlerInstance : StandloneTaskHandler { }

        private class StandaloneStreamFactoryInstance : StandaloneStreamFactory { }
    }
}

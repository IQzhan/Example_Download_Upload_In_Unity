namespace E.Data
{
    public class StandaloneCloner : Cloner
    {
        public StandaloneCloner(System.Uri cacheUri) : base(cacheUri, new StandloneTaskHandlerInstance()) { }
        
        private class StandloneTaskHandlerInstance : StandloneTaskHandler { }
    }
}

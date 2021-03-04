namespace E.Data
{
    public abstract class CloneAsyncOperation : ProcessAsyncOperation
    {
        /// <summary>
        /// load data while downloading? true default.
        /// </summary>
        public bool LoadData
        {
            get { return loadData; }
            set
            {
                if (!IsWorking) { loadData = value; }
                else throw new System.MemberAccessException("can't access LoadData while task is working.");
            }
        }

        private bool loadData = true;

        /// <summary>
        /// loaded data if LoadData is true
        /// </summary>
        public byte[] Data { get; protected set; }

    }
}

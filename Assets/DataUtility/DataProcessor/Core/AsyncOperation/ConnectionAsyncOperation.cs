namespace E.Data
{
    public abstract class ConnectionAsyncOperation : AsyncOperation
    {
        protected ConnectionAsyncOperation() { }

        public struct Account
        {
            public string username;
            public string password;
        }

        /// <summary>
        /// set username and password for source
        /// </summary>
        public Account sourceAccount;

        /// <summary>
        /// set username and password for target
        /// </summary>
        public Account targetAccount;

        /// <summary>
        /// web connection timeout
        /// </summary>
        public int Timeout = 5 * 1000;

        /// <summary>
        /// connecting to source uri?
        /// </summary>
        public bool IsConnecting { get { return IsWorking && !IsProcessing; } }


        /// <summary>
        /// force test host connection, default false
        /// </summary>
        public bool ForceTestConnection
        {
            get { return forceTestConnection; }
            set
            {
                if (!IsWorking) { forceTestConnection = value; }
                else throw new System.MemberAccessException("can't access ForceTestConnection while task is working.");
            }
        }

        private bool forceTestConnection = false;

    }
}

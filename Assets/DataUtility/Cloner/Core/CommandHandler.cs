using System;
using System.Collections.Concurrent;

namespace E.Data
{
    internal class CommandHandler : System.IDisposable
    {
        protected CommandHandler() { }

        private struct Command
        {
            public System.Action body;

            public System.Func<bool> condition;

            public Command(System.Action body, System.Func<bool> condition)
            {
                this.body = body;
                this.condition = condition;
            }

            public void Do()
            {
                if (condition == null || (condition != null && condition()))
                {
                    body();
                }
            }
        }

        private readonly ConcurrentQueue<Command> commands = new ConcurrentQueue<Command>();

        public long MaxFrameMilliseconds { get; set; } = 1000 / 240;

        private long currentMilliseconds;

        public void AddCommand(System.Action body, System.Func<bool> condition = null)
        {
            Enqueue(body, condition);
        }

        private void Enqueue(System.Action body, System.Func<bool> condition = null)
        {
            if (body != null)
            {
                commands.Enqueue(new Command(body, condition));
            }
        }

        public void Tick()
        {
            currentMilliseconds = ClonerClock.Milliseconds;
            while (commands.TryDequeue(out Command commond))
            {
                try
                {
                    commond.Do();
                }
                catch (Exception e)
                {
                    ClonerDebug.LogException(e);
                }
                if (ClonerClock.Milliseconds - currentMilliseconds >= MaxFrameMilliseconds)
                {
                    return;
                }
            }
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                disposedValue = true;
            }
        }

        ~CommandHandler()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}

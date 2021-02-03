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

            public Command(in System.Action body, in System.Func<bool> condition)
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

        private ConcurrentQueue<Command> commands = new ConcurrentQueue<Command>();

        public long MaxFrameMilliseconds { get; set; } = 1000 / 240;

        private long currentMilliseconds;

        public void AddCommand(in System.Action body, in System.Func<bool> condition = null)
        {
            Enqueue(body, condition);
        }

        private void Enqueue(in System.Action body, in System.Func<bool> condition = null)
        {
            if (body != null)
            {
                commands.Enqueue(new Command(body, condition));
            }
        }

        public void Tick()
        {
            if (disposedValue) return;
            currentMilliseconds = DataProcessorClock.Milliseconds;
            while (!commands.IsEmpty && commands.TryDequeue(out Command commond))
            {
                try
                {
                    commond.Do();
                }
                catch (Exception e)
                {
                    DataProcessorDebug.LogException(e);
                }
                if (DataProcessorClock.Milliseconds - currentMilliseconds >= MaxFrameMilliseconds)
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
                    while (!commands.IsEmpty && commands.TryDequeue(out Command commond)) { }
                    commands = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}

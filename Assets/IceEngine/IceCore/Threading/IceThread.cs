using System;
using System.Threading;

namespace IceEngine.Threading
{
    public sealed class IceThread : IDisposable
    {
        public Thread thread;
        public CancellationTokenSource cancelSource;

        public IceThread(Action<CancellationTokenSource> action)
        {
            cancelSource = new CancellationTokenSource();
            thread = new Thread(() => action(cancelSource));
            thread.Start();
        }

        bool disposedValue;

        void DoDispose()
        {
            if (!disposedValue)
            {
                cancelSource.Cancel();
                if (thread != null) thread.Join();
                cancelSource.Dispose();
                thread = null;

                disposedValue = true;
            }
        }

        ~IceThread()
        {
            DoDispose();
        }

        public void Dispose()
        {
            DoDispose();
            GC.SuppressFinalize(this);
        }
    }
}

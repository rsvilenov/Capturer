using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shared.Utils
{
    /// <summary>
    /// https://www.codeproject.com/Tips/709218/Sliding-Timer
    /// </summary>
    public class SlidingTimer
    {
        private readonly int _timeout;
        private readonly object _lock = new Object();

        private bool _isRunning = false;

        public SlidingTimer(int timeout)
        {
            _timeout = timeout;
        }

        public event EventHandler TimeoutElapsed;

        public void Ping()
        {
            lock (_lock)
            {
                if (_isRunning)
                {
                    Monitor.Pulse(_lock);
                }
                else
                {
                    _isRunning = true;
                    Task.Factory.StartNew(WaitForTimeout);
                }
            }
        }

        private void WaitForTimeout()
        {
            lock (_lock)
            {
                while (Monitor.Wait(_lock, _timeout)) ;
                _isRunning = false;
            }

            RaiseTimeoutElapsed();
        }

        private void RaiseTimeoutElapsed()
        {
            var handler = TimeoutElapsed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
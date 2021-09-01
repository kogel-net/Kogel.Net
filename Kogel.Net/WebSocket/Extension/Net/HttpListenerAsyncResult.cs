using System;
using System.Threading;

namespace Kogel.Net.WebSocket.Extension.Net
{
    /// <summary>
    /// 
    /// </summary>
    internal class HttpListenerAsyncResult : IAsyncResult
    {
        private AsyncCallback _callback;
        private bool _completed;
        private bool _completedSynchronously;
        private HttpListenerContext _context;
        private bool _endCalled;
        private Exception _exception;
        private object _state;
        private object _sync;
        private ManualResetEvent _waitHandle;

        internal HttpListenerAsyncResult(AsyncCallback callback, object state)
        {
            _callback = callback;
            _state = state;

            _sync = new object();
        }

        internal HttpListenerContext Context
        {
            get
            {
                if (_exception != null)
                    throw _exception;

                return _context;
            }
        }

        internal bool EndCalled
        {
            get
            {
                return _endCalled;
            }

            set
            {
                _endCalled = value;
            }
        }

        internal object SyncRoot
        {
            get
            {
                return _sync;
            }
        }

        public object AsyncState
        {
            get
            {
                return _state;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                lock (_sync)
                {
                    if (_waitHandle == null)
                        _waitHandle = new ManualResetEvent(_completed);

                    return _waitHandle;
                }
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return _completedSynchronously;
            }
        }

        public bool IsCompleted
        {
            get
            {
                lock (_sync)
                    return _completed;
            }
        }

        private void complete()
        {
            lock (_sync)
            {
                _completed = true;

                if (_waitHandle != null)
                    _waitHandle.Set();
            }

            if (_callback == null)
                return;

            ThreadPool.QueueUserWorkItem(
              state =>
              {
                  try
                  {
                      _callback(this);
                  }
                  catch
                  {
                  }
              },
              null
            );
        }

        internal void Complete(Exception exception)
        {
            _exception = exception;

            complete();
        }

        internal void Complete(HttpListenerContext context, bool completedSynchronously)
        {
            _context = context;
            _completedSynchronously = completedSynchronously;

            complete();
        }
    }
}

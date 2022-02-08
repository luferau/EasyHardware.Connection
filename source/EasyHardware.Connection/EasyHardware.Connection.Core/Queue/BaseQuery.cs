using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Nito.AsyncEx.Synchronous;

namespace EasyHardware.Connection.Core.Queue
{
    public abstract class BaseQuery : IDisposable
    {
        public abstract void SetException(Exception ex);

        public abstract void Dispose();
    }

    public abstract class BaseQueryTcs<T> : BaseQuery
    {
        // The object used for synchronization.
        private readonly object _mutex;

        // The current state of the event.
        private TaskCompletionSource<object> _tcs;

        private Exception _exception;
        private T _result;

        /// <summary>
        /// Creates an async-compatible manual-reset event.
        /// </summary>
        protected BaseQueryTcs()
        {
            _mutex = new object();
            _tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// Whether this event is currently set. This member is seldom used; code using this member has a high possibility of race conditions.
        /// </summary>
        public bool IsSet
        {
            get { lock (_mutex) return _tcs.Task.IsCompleted; }
        }

        /// <summary>
        /// Asynchronously waits for this event to be set.
        /// </summary>
        public Task WaitResultAsync()
        {
            lock (_mutex)
            {
                return _tcs.Task;
            }
        }

        /// <summary>
        /// Asynchronously waits for this event to be set or for the wait to be canceled.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this token is already canceled, this method will first check whether the event is set.</param>
        public Task WaitResultAsync(CancellationToken cancellationToken)
        {
            var waitTask = WaitResultAsync();
            return waitTask.IsCompleted ? waitTask : waitTask.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Synchronously waits for this event to be set. This method may block the calling thread.
        /// </summary>
        public void WaitResult()
        {
            WaitResultAsync().WaitAndUnwrapException();
        }

        /// <summary>
        /// Synchronously waits for this event to be set. This method may block the calling thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token used to cancel the wait. If this token is already canceled, this method will first check whether the event is set.</param>
        public void WaitResult(CancellationToken cancellationToken)
        {
            var ret = WaitResultAsync(cancellationToken);
            if (ret.IsCompleted)
                return;
            ret.WaitAndUnwrapException(cancellationToken);
        }

        public T GetResult()
        {
            if (_exception != null)
                throw new Exception("Exception during query processing. See inner exception for details", _exception);
            return _result;
        }

        /// <summary>
        /// Sets the event, atomically completing every task returned by <see cref="O:Nito.AsyncEx.AsyncManualResetEvent.WaitAsync"/>. If the event is already set, this method does nothing.
        /// </summary>
        /// /// <param name="result">Result object</param>
        public void SetResult(T result)
        {
            lock (_mutex)
            {
                _result = result;
                _tcs.TrySetResult(null);
            }
        }

        /// <summary>
        /// Sets the event, atomically completing every task returned by <see cref="O:Nito.AsyncEx.AsyncManualResetEvent.WaitAsync"/>. If the event is already set, this method does nothing.
        /// </summary>
        /// /// <param name="result">Result object</param>
        public override void SetException(Exception ex)
        {
            lock (_mutex)
            {
                _exception = ex;
                _tcs.TrySetResult(null);
            }
        }

        /// <summary>
        /// Resets the event. If the event is already reset, this method does nothing.
        /// </summary>
        public void Reset()
        {
            lock (_mutex)
            {
                if (_tcs.Task.IsCompleted)
                    _tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            }
        }

        public override void Dispose()
        {

        }
    }
}

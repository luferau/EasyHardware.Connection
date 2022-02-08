using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EasyHardware.Connection.Core.Enums;
using EasyHardware.Connection.Core.Events;
using EasyHardware.Connection.Core.Queue;
using EasyHardware.Connection.Core.Support;

namespace EasyHardware.Connection.Core
{
    public abstract class ConnectionBase : IConnection
    {
        #region Queue private variables

        private readonly BlockingCollection<BaseQuery> _queries = new BlockingCollection<BaseQuery>(new ConcurrentQueue<BaseQuery>());
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Dictionary<Type, Action<BaseQuery>> _handlers = new Dictionary<Type, Action<BaseQuery>>();

        #endregion

        private readonly List<byte> _inBuffer;

        #region Constructor

        protected ConnectionBase()
        {
            this._inBuffer = new List<byte>();

            // Add handlers for different type of queries
            this._handlers.Add(typeof(WriteQuery), WriteQueryHandler);
            this._handlers.Add(typeof(WriteReadQuery), WriteReadQueryHandler);
            this._handlers.Add(typeof(ReadQuery), ReadQueryHandler);

            // Start queries processing loop
            new Thread(this.QueriesProcessingLoop) { IsBackground = true }.Start();
        }

        #endregion

        #region IConnection properties

        public abstract bool IsOpen { get; }

        public virtual string FullName => this.ShortName;

        public abstract string ShortName { get; }

        public int ReadTimeout_ms { get; set; }

        public AnswerReceiveConditionType ReceiveCondition { get; set; }

        public string SpecifiedStringPattern
        {
            get => this.SpecifiedByteDataPattern.ToAsciiString();
            set => this.SpecifiedByteDataPattern = value.ToAsciiBytes();
        }

        public byte[] SpecifiedByteDataPattern { get; set; }

        public int SpecifiedDataAmount { get; set; }

        #endregion

        #region IConnection methods

        public abstract bool Open();

        public abstract void Close();

        public bool OpenIfClosed()
        {
            if (!this.IsOpen)
            {
                var openResult = this.Open();

                if (!openResult)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<ConnectionResultType> WriteAsync(string text)
        {
            return await WriteAsync(text.ToAsciiBytes());
        }

        public async Task<ConnectionResultType> WriteAsync(byte[] data)
        {
            if (!this.IsOpen)
            {
                return ConnectionResultType.ErrorConnectionClosed;
            }

            // Create write query
            var writeQuery = new WriteQuery
            {
                WriteData = data
            };

            // Add write query to queue
            this.AddQuery(writeQuery);

            try
            {
                // Waiting for data to be write
                await writeQuery.WaitResultAsync();

                var result = writeQuery.GetResult();

                return result ? ConnectionResultType.Ok : ConnectionResultType.ErrorUndefined;
            }
            catch (Exception)
            {
                return ConnectionResultType.ErrorUndefined;
            }
        }

        public async Task<(ConnectionResultType, string)> WriteReadAsync(string text)
        {
            var (result, data) = await WriteReadAsync(text.ToAsciiBytes());

            if (result == ConnectionResultType.Ok)
            {
                return (result, data.ToAsciiString());
            }

            return (result, string.Empty);
        }

        public async Task<(ConnectionResultType, byte[])> WriteReadAsync(byte[] data)
        {
            if (!this.IsOpen)
            {
                return (ConnectionResultType.ErrorConnectionClosed, Array.Empty<byte>());
            }

            // Create write/read query
            var writeReadQuery = new WriteReadQuery
            {
                WriteData = data,
                ReadTimeout_ms = this.ReadTimeout_ms,
                ReceiveCondition = this.ReceiveCondition,
                SpecifiedStringPattern = this.SpecifiedStringPattern,
                SpecifiedByteDataPattern = this.SpecifiedByteDataPattern,
                SpecifiedDataAmount = this.SpecifiedDataAmount
            };

            // Add write/read query to queue
            this.AddQuery(writeReadQuery);

            try
            {
                // Waiting for query finished
                await writeReadQuery.WaitResultAsync();

                var result = writeReadQuery.GetResult();

                return result;
            }
            catch (Exception)
            {
                return (ConnectionResultType.ErrorUndefined, Array.Empty<byte>());
            }
        }

        public void Dispose()
        {
            this._cts.Cancel();
        }

        #endregion

        protected abstract void WriteInternal(byte[] data);

        protected abstract byte[] ReadInternal();

        protected virtual void BeforeWriteRead(bool discardBuffers = true) { }

        protected virtual void AfterWriteRead() { }

        #region Queries Queue

        private void QueriesProcessingLoop()
        {
            try
            {
                foreach (var query in this._queries.GetConsumingEnumerable(this._cts.Token))
                {
                    try
                    {
                        this._handlers[query.GetType()](query);
                    }
                    catch (Exception ex)
                    {
                        query.SetException(ex);
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void AddQuery(BaseQuery query)
        {
            if (!this._handlers.ContainsKey(query.GetType()))
                throw new Exception("No handlers registered for query type " + query.GetType());

            this._queries.Add(query);
        }

        private void WriteQueryHandler(BaseQuery baseQuery)
        {
            // Cast to WriteQuery
            var writeQuery = (WriteQuery)baseQuery;

            // Process query
            try
            {
                // Write to connections
                this.WriteInternal(writeQuery.WriteData);

                // Set query result
                writeQuery.SetResult(true);
            }
            catch (Exception exception)
            {
                writeQuery.SetException(exception);
            }
        }

        private async void WriteReadQueryHandler(BaseQuery baseQuery)
        {
            // Cast to WriteReadQuery
            var writeReadQuery = (WriteReadQuery)baseQuery;

            // Start write/read process...
            this.BeforeWriteRead();

            // Write request command
            this.WriteInternal(writeReadQuery.WriteData);

            // Wait for answer or timeout
            var cancellationTokenSource = new CancellationTokenSource(writeReadQuery.ReadTimeout_ms);

            var result = await Task.Run(() => this.DoReadAnswer(writeReadQuery, cancellationTokenSource.Token), cancellationTokenSource.Token);

            if (result)
            {
                var answer = this._inBuffer.ToArray();

                // Cancel WriteReadQuery with success
                writeReadQuery.SetResult((ConnectionResultType.Ok, answer));

                // Throw AnswerReceived event
                this.OnAnswerReceived(new TransmitReceiveDataEventArgs(DataDirectionType.Receive, answer));
            }
            else if (cancellationTokenSource.IsCancellationRequested)
            {
                // Cancel WriteReadQuery with timeout error
                writeReadQuery.SetResult((ConnectionResultType.ErrorReadTimeout, Array.Empty<byte>()));

                this.OnCommunicationEventOccurred(new CommunicationEventArgs(CommunicationEventType.ReadTimeoutOccur));

                // Show what we received
                if (this._inBuffer.Count > 0)
                {
                    // Throw AnswerReceived event
                    this.OnAnswerReceived(new TransmitReceiveDataEventArgs(DataDirectionType.Receive, this._inBuffer.ToArray()));
                }
            }
            else
            {
                // Cancel WriteReadQuery with undefined error
                writeReadQuery.SetResult((ConnectionResultType.ErrorUndefined, Array.Empty<byte>()));
            }

            this.AfterWriteRead();
        }

        private async void ReadQueryHandler(BaseQuery baseQuery)
        {
            // Cast to ReadQuery
            var readQuery = (ReadQuery)baseQuery;

            // Start read process query...
            this.BeforeWriteRead();

            // Wait for answer or timeout
            var cancellationTokenSource = new CancellationTokenSource(readQuery.ReadTimeout_ms);

            var result = await Task.Run(() => this.DoReadAnswer(readQuery, cancellationTokenSource.Token), cancellationTokenSource.Token);

            if (result)
            {
                var answer = this._inBuffer.ToArray();

                // Cancel WriteReadQuery with success
                readQuery.SetResult((ConnectionResultType.Ok, answer));

                // Throw AnswerReceived event
                this.OnAnswerReceived(new TransmitReceiveDataEventArgs(DataDirectionType.Receive, answer));
            }
            else if (cancellationTokenSource.IsCancellationRequested)
            {
                // Cancel WriteReadQuery with timeout error
                readQuery.SetResult((ConnectionResultType.ErrorReadTimeout, Array.Empty<byte>()));

                this.OnCommunicationEventOccurred(new CommunicationEventArgs(CommunicationEventType.ReadTimeoutOccur));

                // Show what we received
                if (this._inBuffer.Count > 0)
                {
                    // Throw AnswerReceived event
                    this.OnAnswerReceived(new TransmitReceiveDataEventArgs(DataDirectionType.Receive, this._inBuffer.ToArray()));
                }
            }
            else
            {
                // Cancel WriteReadQuery with undefined error
                readQuery.SetResult((ConnectionResultType.ErrorUndefined, Array.Empty<byte>()));
            }

            this.AfterWriteRead();
        }

        private bool DoReadAnswer(IReadQuery readQuery, CancellationToken cancelToken)
        {
            // Clear input buffer
            this._inBuffer.Clear();

            // Answer from device received flag 
            var answerReceived = false;

            while (!answerReceived && !cancelToken.IsCancellationRequested)
            {
                this._inBuffer.AddRange(this.ReadInternal());

                switch (readQuery.ReceiveCondition)
                {
                    case AnswerReceiveConditionType.SpecifiedDataAmount:
                        if (this._inBuffer.Count >= readQuery.SpecifiedDataAmount)
                        {
                            answerReceived = true;
                        }
                        break;

                    case AnswerReceiveConditionType.AnyData:
                        if (this._inBuffer.Count > 0)
                        {
                            answerReceived = true;
                        }
                        break;

                    case AnswerReceiveConditionType.NewLine:
                        if (this._inBuffer.Contains(0x0D) && this._inBuffer.Contains(0x0A))
                        {
                            answerReceived = true;
                        }
                        break;

                    case AnswerReceiveConditionType.SpecifiedString:
                    case AnswerReceiveConditionType.SpecifiedByteData:
                        if (this._inBuffer.ToArray().Locate(readQuery.SpecifiedByteDataPattern).Length > 0)
                        {
                            answerReceived = true;
                        }
                        break;

                    case AnswerReceiveConditionType.CorrectCrc:
                        throw new NotImplementedException();
                }
            }
            return answerReceived;
        }

        #endregion

        #region Events

        public event EventHandler<TransmitReceiveDataEventArgs> DataTransmit;

        protected virtual void OnDataTransmit(TransmitReceiveDataEventArgs args)
        {
            args.Record.DataDirection = DataDirectionType.Transmit;
            args.Record.ConnectionName = this.ShortName;
            this.DataTransmit?.Invoke(this, args);
        }

        public event EventHandler<TransmitReceiveDataEventArgs> AnswerReceived;

        protected virtual void OnAnswerReceived(TransmitReceiveDataEventArgs args)
        {
            args.Record.DataDirection = DataDirectionType.Receive;
            args.Record.ConnectionName = this.ShortName;
            this.AnswerReceived?.Invoke(this, args);
        }

        public event EventHandler<TransmitReceiveDataEventArgs> DataReceived;

        protected virtual void OnDataReceived(TransmitReceiveDataEventArgs args)
        {
            args.Record.ConnectionName = ShortName;
            this.DataReceived?.Invoke(this, args);
        }

        public event EventHandler<ConnectionStatusEventArgs> StatusChanged;

        protected virtual void OnStatusChanged(ConnectionStatusEventArgs args)
        {
            this.StatusChanged?.Invoke(this, args);
        }


        public event EventHandler<CommunicationEventArgs> CommunicationEventOccurred;

        protected virtual void OnCommunicationEventOccurred(CommunicationEventArgs e)
        {
            this.CommunicationEventOccurred?.Invoke(this, e);
        }

        protected virtual void ExceptionHandler(string message, string title)
        {
            this.OnStatusChanged(new ConnectionStatusEventArgs(ConnectionStateType.Faulty, message));
        }

        #endregion Events
    }
}

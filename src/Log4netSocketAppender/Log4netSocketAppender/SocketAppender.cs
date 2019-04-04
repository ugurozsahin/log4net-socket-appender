using log4net.Core;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace log4net.Appender
{
    public class SocketAppender : AppenderSkeleton
    {
        public SocketAppender()
        {
            AddressFamily = AddressFamily.InterNetwork;
            SocketType = SocketType.Stream;
            ProtocolType = ProtocolType.Tcp;
            ConAttemptsCount = 3;
            ConAttemptsWaitingTimeMilliSeconds = 1000;
            UseThreadPoolQueue = false;
        }
        public string RemoteAddress { get; set; }
        public int RemotePort { get; set; }
        public bool DebugMode { get; set; }
        public AddressFamily AddressFamily { get; set; }
        public SocketType SocketType { get; set; }
        public ProtocolType ProtocolType { get; set; }
        public int ConAttemptsCount { get; set; }
        public int ConAttemptsWaitingTimeMilliSeconds { get; set; }
        public bool UseThreadPoolQueue { get; set; }
        public int ReconnectTimeInSeconds { get; set; }

        private static DateTime? _nextTrialTime = null;
        private Socket _socket;

        public override void ActivateOptions()
        {
            if (_nextTrialTime.HasValue && _nextTrialTime.Value > DateTime.Now) return;
            else _nextTrialTime = null;
            
            var retryCount = 0;
            while (++retryCount <= ConAttemptsCount)
            {
                try
                {
                    _socket = new Socket(AddressFamily, SocketType, ProtocolType);
                    _socket.Connect(RemoteAddress, RemotePort);
                    break;
                }
                catch (ArgumentNullException argumentNullException)
                {
                    Console.WriteLine("ArgumentNullException : {0}", argumentNullException);
                }
                catch (SocketException socketException)
                {
                    Console.WriteLine("SocketException : {0}", socketException);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Unexpected exception : {0}", exception);
                }
                Thread.Sleep(ConAttemptsWaitingTimeMilliSeconds);
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (UseThreadPoolQueue)
                ThreadPool.QueueUserWorkItem(state => AppendLog(loggingEvent));
            else
                AppendLog(loggingEvent);

        }

        private void AppendLog(LoggingEvent loggingEvent)
        {
            var rendered = string.Empty;

            if (_socket.Connected)
            {
                rendered = RenderLoggingEvent(loggingEvent);

                var msg = Encoding.UTF8.GetBytes(rendered);

                var bytesSent = _socket.Send(msg);

                if (DebugMode)
                {
                    Console.WriteLine("- Bytes sent: " + bytesSent);
                }
            }
            else
            {
                Console.WriteLine("[UNSUCCESSFULL]:: " + rendered);
                if (!_nextTrialTime.HasValue)
                {
                    _nextTrialTime = DateTime.Now.AddSeconds(ReconnectTimeInSeconds);
                }
                ActivateOptions();
            }
        }

        protected override void OnClose()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
    }
}


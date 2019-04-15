using log4net;
using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace log4net.Appender
{
    public class SocketAppenderAsync : AppenderSkeleton
    {
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);

        public SocketAppenderAsync()
        {
            AddressFamily = AddressFamily.InterNetwork;
            SocketType = SocketType.Stream;
            ProtocolType = ProtocolType.Tcp;
            ConAttemptsCount = 3;
            ConAttemptsWaitingTimeMilliSeconds = 1000;
            ReconnectTimeInSeconds = 600;
            UseThreadPoolQueue = false;
            DebugMode = false;
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

                    _socket.BeginConnect(RemoteAddress, RemotePort, new AsyncCallback(ConnectCallback), _socket);

                    connectDone.WaitOne(5000, true);

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

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket _socket = (Socket)ar.AsyncState;

                _socket.EndConnect(ar);

                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());               
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

                try
                {
                    Send(_socket, rendered);

                    sendDone.WaitOne();
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
            }
            else
            {
                Console.WriteLine("[UNSUCCESSFULL]:: Can not connect to server." + rendered);
                if (!_nextTrialTime.HasValue)
                {
                    _nextTrialTime = DateTime.Now.AddSeconds(ReconnectTimeInSeconds);
                }
                ActivateOptions();
            }
        }

        private static void Send(Socket client, String data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket _socket = (Socket)ar.AsyncState;

                int bytesSent = _socket.EndSend(ar);

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        protected override void OnClose()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
    }
}

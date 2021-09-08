using System;
using System.IO;
using System.Net.Sockets;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace NeosGateway
{
    public class CiliaServer
    {
        static private int mCiliaPort = 1995;
        //tcp/ip ip address of the SDK. May make this variable in the future when SDK may be on other machine.
        static private string mCiliaIP = "localhost";
        static private CiliaServer ciliaServer;
        private TcpClient mCiliaClient = new TcpClient();
        private NetworkStream mCiliaStream;
        private System.IO.StreamReader mStreamReader;
        

        public static CiliaServer GetCiliaServer()
        {
            if(ciliaServer == null)
            {
                ciliaServer = new CiliaServer();
            }
            return ciliaServer;
        }
        CiliaServer()
        {
            if (!mCiliaClient.Connected)
                try
                {
                    mCiliaClient = new TcpClient();
                    mCiliaClient.Connect(mCiliaIP, mCiliaPort);
                    mCiliaStream = mCiliaClient.GetStream();
                    mStreamReader = new StreamReader(mCiliaStream);
                    Console.WriteLine("Cilia Connected");
                }
                catch
                {
                    Console.WriteLine("Cilia Not Connected");
                    return;
                }
        }
        public void SendMessageToCilia(string aMessageToSend)
        {
            byte[] message = System.Text.Encoding.ASCII.GetBytes(aMessageToSend);

            mCiliaStream.Write(message, 0, message.Length);
        }
        public void ShutDown()
        {
            if (mCiliaStream != null)
                mCiliaStream.Close();
            if (mCiliaClient != null)
                mCiliaClient.Close();
        }
        ~CiliaServer()
        {
            if (mCiliaStream != null)
                mCiliaStream.Close();
            if (mCiliaClient != null)
                mCiliaClient.Close();
        }
    }
    public class Echo : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("Received message from clinet: " + e.Data);
            if (e.Data.Contains("["))
                if (e.Data.Contains("]"))
                {
                    CiliaServer.GetCiliaServer().SendMessageToCilia(e.Data);
                }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            WebSocketServer webSocket = new WebSocketServer("ws://127.0.0.1:80");
            webSocket.AddWebSocketService<Echo>("/Echo");
            webSocket.Start();

            Console.WriteLine("Ws server started on ws://127.0.0.1:80");

            Console.ReadKey();
            CiliaServer.GetCiliaServer().ShutDown();
            webSocket.Stop();
            
        }
    }
}

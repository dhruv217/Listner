using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using TeltonikaServer.Teltonika;

namespace TeltonikaServer
{
    class Program
    {
         static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 34400);
            TcpClient client;
            listener.Start();

            while (true) // Add your exit flag here
            {
                client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(ThreadProc, client);
            }
        }

        private static void ThreadProc(object state)
        {
            string IMEI = "";
            var client = ((TcpClient)state);
            NetworkStream nwStream = ((TcpClient)state).GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];
            bool imeiRecieved =false;
            while (client.Connected)
            {
                try
                {
                    int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (!imeiRecieved)
                    {
                        IMEI = dataReceived;
                        Console.WriteLine("IMEI received : " + dataReceived);

                        Byte[] result = {(byte) 0x01};
                        nwStream.Write(result, 0, 1);
                        imeiRecieved = true;
                    }
                    else
                    {
                        var parser = new TeltonikaDevicesParser(false);
                        var result = parser.Decode(new List<byte>(buffer), IMEI);
                        nwStream.Write(
                            result > 0 ? new byte[] {0x00, 0x00, 0x00, 0x01} : new byte[] {0x00, 0x00, 0x00, 0x00}, 0,
                            4);
                    }
                }
                catch (Exception e)
                {
                   // Console.WriteLine(e);
                    client.Close();
                    //throw;
                }
            }
            //throw new NotImplementedException();
        }

    }
}

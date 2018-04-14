using System;
using System.Collections.Generic;
using System.Linq;
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
            string imei = string.Empty;
            var client = ((TcpClient) state);
            NetworkStream nwStream = ((TcpClient) state).GetStream();
            byte[] buffer = new byte[client.ReceiveBufferSize];

            try
            {
              

                while (true)
                {
                    int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    if (imei == string.Empty)
                    {
                        imei = dataReceived;
                        Console.WriteLine("IMEI received : " + dataReceived);

                        Byte[] b = { 0x01 };
                        nwStream.Write(b, 0, 1);

                    }
                    else
                    {
                        int dataNumber =  Convert.ToInt32(buffer.Skip(9).Take(1).ToList()[0]); ;

                        var result = 0;
                        while (dataNumber > 0)
                        {
                            var parser = new TeltonikaDevicesParser(false);
                            result = parser.Decode(new List<byte>(buffer), imei);
                            dataNumber--;
                        }
                        nwStream.Write(
                            result > 0 ? new byte[] { 0x00, 0x00, 0x00, 0x01 } : new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0,
                            4);
                    }
                }




            }
            catch (Exception e)
            {
                // Console.WriteLine(e);
                client.Close();
                //throw;
            }

            //throw new NotImplementedException();
        }

    }
}

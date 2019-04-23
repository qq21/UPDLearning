using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UPDLearning
{
    class Client
    {
        private UdpClient _udpClient;

        public bool isConnect
        {
            get
            {
                return _udpClient.Client.Connected;
            }
        }


        public Client(IPEndPoint iPEndPoint)
        {
            _udpClient = new UdpClient(iPEndPoint);
        }
    }

    class Program
    {
        static UdpClient udpServer = new UdpClient(new IPEndPoint(IPAddress.Parse("192.168.1.121"), 7789));
        static List<IPEndPoint> Cliens = new List<IPEndPoint>();
       private static byte[] data;
        static void Main(string[] args)
        {

       

            //IPEndPoint iPEnd=new IPEndPoint(IPAddress.Parse("192.168.1.122"),7788);
                
            Thread recevieT = new Thread( Receive);
            recevieT.Start();

            //while (true)
            //{
            //    string mes = Console.ReadLine();
            //    byte[] data = Encoding.UTF8.GetBytes(mes);
            //    udpClient.Send(data, data.Length, iPEnd);
            //}
            
        }

        static void Receive()
        {

        
            Console.WriteLine("服务器启动成功,等待玩家连接");
            while (true)
            {

                if (udpServer.Client.Poll(10, SelectMode.SelectRead))
                {

                    IPEndPoint iPEnd = new IPEndPoint(IPAddress.Any, 0);
                
                    try
                    {
                        data = udpServer.Receive(ref iPEnd);
                        string[] datas = Encoding.UTF8.GetString(data).Split('@');

                        //Todo:判断字典中 是否存在该 UDP
                        if (!Cliens.Contains(iPEnd))
                        {
                            Console.WriteLine(iPEnd.Address + ":" + iPEnd.Port + "连接进来了");

                            Cliens.Add(iPEnd);
                        }

                        //TODO: 收到 掉线命令，然后在字典里进行处理 判断是否存在  如果 存在 进行移除;
                         
                        string mes = datas[1];

                        Console.WriteLine("从" + iPEnd.Address + "端口:" + iPEnd.Port + "收到了数据：" + mes);

                        // TODO 进行广播 遍历连接进来的客服端
                        Boracast(data, iPEnd);
                    }
                    catch (Exception e)
                    {
                        if (e.GetType() == typeof(SocketException))
                        {
                            //Console.WriteLine(udpServer.Available);
                            //Console.WriteLine("断开连接" + iPEnd.Address.ToString());
                        }
                    }

                }
            }
        }

        /// <summary>
        ///  对 客服端进行 广播， 判断是否处于连接 状态
        /// </summary>
        /// <param name="data"></param>
        /// <param name="iPEnd"></param>
        static void Boracast(byte [] data,IPEndPoint iPEnd)
        {
            foreach (var client in Cliens)
            {
                if (client!=iPEnd)
                {                       
                    udpServer.Send(data, data.Length, client);
                }
            
            }
        }

        void ConnectServer()
        {

            UdpClient udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(GetIpAddress()), 7788));

            IPEndPoint iPEnd = new IPEndPoint(IPAddress.Any, 0);

            byte[] data = udpClient.Receive(ref iPEnd);

            string mes = Encoding.UTF8.GetString(data);
            Console.WriteLine("从" + iPEnd.Address + "端口:" + iPEnd.Port + "收到了数据：" + mes);

            udpClient.Send(data, data.Length, iPEnd);
        }

        
        private static string GetIpAddress()
        {
            string hostName = Dns.GetHostName();   //获取本机名
            IPHostEntry localhost = Dns.GetHostByName(hostName);    //方法已过期，可以获取IPv4的地址
            //IPHostEntry localhost = Dns.GetHostEntry(hostName);   //获取IPv6地址
            IPAddress localaddr = localhost.AddressList[0];

            return localaddr.ToString();
        }
    }
 
}

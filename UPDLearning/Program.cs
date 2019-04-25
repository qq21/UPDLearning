using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UPDLearning
{
 

    class Program
    {
        private static string ip = "193.112.3.26";    //"192.168.1.121";

        static UdpClient udpServer;

        static List<IPEndPoint> Cliens = new List<IPEndPoint>();

        private static Queue<byte[]> mesQueue = new Queue<byte[]>(); //利用消息队列  进行消息分发，最好有个 消息中心的类
        ///TODO:  利用这个基本 去搭建一个 简易版架构，消息中心处理  数据类模型 

        private static byte[] data;


        static void Main(string[] args)
        {
            ip = GetIpAddress();
            udpServer = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), 7789));


            Thread recevieT = new Thread(Receive);
            recevieT.Start();


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
                        //进消息队列 
                        // 判断字典中 是否存在该 UDP
                        if (!Cliens.Contains(iPEnd))
                        {
                            Console.WriteLine(iPEnd.Address + ":" + iPEnd.Port + "连接进来了");

                            Cliens.Add(iPEnd);
                        }

                        string mes = datas[1];

                        if (Cliens.Count>1)                
                        Boracast(data, iPEnd);

                        Console.WriteLine("从" + iPEnd.Address + "端口:" + iPEnd.Port + "收到了数据：" + mes);
                        //掉线指令 移除
                        if (mes.Split(':')[0] == "0")
                        {
                            Cliens.Remove(iPEnd);
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.GetType() == typeof(SocketException))
                        {

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
        static void Boracast(byte[] data, IPEndPoint iPEnd)
        {
            foreach (var client in Cliens)
            {
                if (client != iPEnd)
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

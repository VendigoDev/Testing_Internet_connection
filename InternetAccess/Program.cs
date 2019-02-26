using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;

namespace InternetAccess
{
    class Program
    {
        static List<string> serviceList = new List<string>() 
        { "vk.com", "yandex.ru", "google.com", "youtube.com", "twitter.com"}; // Список сервисов

        static List<string> InterfacesList = new List<string>(); // Список интерфейсов

        public static int Ping(List<string> list)
        {
            int wrong = 0;

            Ping p = new Ping();

            foreach (string server in list)
            {
                try
                {
                    PingReply pr = p.Send(server);
                    // server
                    var address = pr.Address; // IP adress
                    var status = pr.Status; // Статус
                    var time = pr.RoundtripTime; // Время ответа
                    var ttl = pr.Options.Ttl; //TTL

                    if (status == IPStatus.Success)
                    {
                        Console.WriteLine("Сервер: {0}, IP адрес: {1}, Статус: {2}, Пинг: {3}, TTL: {4}, Дата: {5}", server, pr.Address, status, time, ttl, DateTime.Now);
                    }

                }
                catch
                {
                    wrong++;
                    Console.WriteLine("Сервер {0} не доступен! Дата: {1}", server, DateTime.Now);
                }
                
            }


            return wrong;
        } // Пинг списка

        public static void GetHostIPnGate()
        {
            foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()).Where(ha => ha.AddressFamily == AddressFamily.InterNetwork))
            {
                Console.WriteLine($"IP: {ip}");


                var targetInterface = NetworkInterface.GetAllNetworkInterfaces().SingleOrDefault(ni => ni.GetIPProperties().UnicastAddresses.OfType<UnicastIPAddressInformation>().Any(x => x.Address.Equals(ip)));
                if (targetInterface == null)
                    continue;

                var gates = targetInterface.GetIPProperties().GatewayAddresses;
                if (gates.Count == 0)
                {
                    Console.WriteLine("Шлюз : отсутствует!");
                    Console.WriteLine("Возможно не исправна или не подключена сетевая карта!");
                    Console.WriteLine(Environment.NewLine);
                    continue;
                }

                Console.WriteLine("Шлюз :");
                foreach (var gateAddress in gates)
                {
                    Console.WriteLine($"{gateAddress.Address:10}");
                }
                Console.WriteLine(Environment.NewLine);
            }
        } // Получить IP и шлюз ПК

        public static void GetInterfaces()
        {
            NetworkInterface[] intf = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface device in intf)
            {
                Console.WriteLine();
                Console.WriteLine("MAC адрес:");
                Console.WriteLine(device.GetPhysicalAddress());
                IPAddress ipv6Address = device.GetIPProperties().UnicastAddresses[0].Address; //This will give ipv6 address of certain adapter 
                Console.WriteLine(ipv6Address);
                IPAddress ipv4Address = device.GetIPProperties().UnicastAddresses[1].Address;//This will give ipv4 address of certain adapter
                InterfacesList.Add(ipv4Address.ToString()); // Добавляем IP в список
                Console.WriteLine(ipv4Address);

                Console.WriteLine("Шлюз:");
                foreach (var gateway in device.GetIPProperties().GatewayAddresses)
                {
                    Console.WriteLine(gateway.Address);
                }

            }
        } // Получить список интерфейсов

        static void Main(string[] args)
        {

            int count = Ping(serviceList); // Пинг списка сервисов

            if (count < serviceList.Count) { 
                Console.WriteLine("Сервисов не отвечает: {0}, соединение с интернетом присутствует!", count);
                
            }

            else{
                Console.WriteLine("Соединение с интернетом отсутствует, возможно отключен или неисправен роутер!");
                Console.WriteLine();

                Console.WriteLine("Получаем список сетевых интерфейсов: ");
                GetInterfaces();
                Console.WriteLine();

                Console.WriteLine("Список ip интерфейсов: ");
                foreach (var ip in InterfacesList)
                {
                    Console.WriteLine(ip);
                }

                Console.WriteLine();
                Console.WriteLine("Пинг ip интерфейсов: ");
                int count2 = Ping(InterfacesList); // Пинг списка ip интерфейсов
                Console.WriteLine("Не доступно {0} устройств из {1}", count2, InterfacesList.Count);

                Console.WriteLine();
                Console.WriteLine($"Получаем IP и шлюз ПК :");
                GetHostIPnGate();
            }



            Console.Read();
        }
    }
}

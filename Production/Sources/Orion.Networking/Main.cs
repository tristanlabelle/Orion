using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Orion.Networking
{
    class Program
    {
        private const int port = 41223;

        static void Main()
        {
            IPAddress address;
            IPAddress.TryParse(Console.ReadLine(), out address);
            IPEndPoint endPoint = new IPEndPoint(address, port);

            using (Transporter transporter = new Transporter(port))
            {
                int count = 0;
                Dictionary<int, bool> received = new Dictionary<int,bool>();
                for (int i = 0; i < 0xFF; i++)
                {
                    transporter.SendTo(BitConverter.GetBytes(i), endPoint);
                }

                transporter.Received += delegate(Transporter origin, NetworkEventArgs args)
                {
                    int id = BitConverter.ToInt32(args.Data, 0);
                    Console.WriteLine("{0}: {1}", args.Host, id);
                    received[id] = true;
                    count++;
                };

                transporter.TimedOut += delegate(Transporter origin, NetworkTimeoutEventArgs args)
                {
                    Console.WriteLine("*** {0} timed out", BitConverter.ToInt32(args.Data, 0));
                    transporter.SendTo(args.Data, args.Host);
                };

                while (count != 0xFF)
                {
                    transporter.Poll();
                }
            }
            Console.Read();
        }
    }
}

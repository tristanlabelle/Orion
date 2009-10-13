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
                int received = 0;

                for (int i = 0; i < 0xFF; i++)
                {
                    transporter.SendTo(BitConverter.GetBytes(i), endPoint);
                }

                transporter.Received += delegate(Transporter origin, NetworkEventArgs args)
                {
                    Console.WriteLine("{0}: {1}", args.Host, BitConverter.ToInt32(args.Data, 0));
                    received++;
                };

                transporter.TimedOut += delegate(Transporter origin, NetworkTimeoutEventArgs args)
                {
                    transporter.SendTo(args.Data, args.Host);
                };

                while (received < 0xFF)
                {
                    transporter.Poll();
                }
            }
        }
    }
}

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
            using (Transporter transporter = new Transporter(port))
            {
                transporter.Received += delegate(Transporter source, NetworkEventArgs args)
                {
                    Console.WriteLine(Encoding.ASCII.GetString(args.Data, 0, args.Data.Length));
                };

                transporter.TimedOut += delegate(Transporter source, NetworkTimeoutEventArgs args)
                {
                    Console.WriteLine("*** Message timed out: {0}", Encoding.ASCII.GetString(args.Data));
                };

                while (true)
                {
                    Console.Write("Adresse: ");
                    IPAddress address;
                    IPAddress.TryParse(Console.ReadLine(), out address);
                    IPEndPoint endPoint = new IPEndPoint(address, port);

                    Console.Write("Message: ");
                    string message = Console.ReadLine();
                    transporter.SendTo(Encoding.ASCII.GetBytes(message), new IPEndPoint(address, port));
                    transporter.Poll();
                }
            }
        }
    }
}

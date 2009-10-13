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

            using (Transporter transporter = new Transporter(port))
            {
                transporter.Received += delegate(Transporter origin, NetworkEventArgs args)
                {
                    Console.WriteLine("{0}: {1}", args.Host, Encoding.ASCII.GetChars(args.Data));
                };

                while (true)
                {
                    transporter.SendTo(Encoding.ASCII.GetBytes(Console.ReadLine()), new IPEndPoint(address, port));
                    transporter.Poll();
                }
            }
        }
    }
}

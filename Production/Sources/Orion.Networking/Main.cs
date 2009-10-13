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
            Transporter transporter = new Transporter(port);
            transporter.Received += delegate(Transporter origin, NetworkEventArgs args)
            {
                Console.WriteLine(Encoding.ASCII.GetChars(args.Data));
            };

            while (true)
            {
                IPAddress address;
                IPAddress.TryParse(Console.ReadLine(), out address);
                transporter.SendTo(Encoding.ASCII.GetBytes(Console.ReadLine()), new IPEndPoint(address, port));
                transporter.Poll();
            }
        }
    }
}

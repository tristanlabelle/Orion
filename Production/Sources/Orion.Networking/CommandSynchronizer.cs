using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

using Orion.GameLogic;
using Orion.Commandment;
using Orion.Commandment.Commands;

namespace Orion.Networking
{
    public enum GameMessageType : byte
    {
        Commands,
        Done
    }

    public class CommandSynchronizer : CommandSink, IDisposable
    {
        private static int frameModulo = 6;

        private Transporter transporter;
        private Dictionary<IPEndPoint, bool> peersCompleted = new Dictionary<IPEndPoint, bool>();

        public CommandSynchronizer(Transporter transporter, IEnumerable<IPEndPoint> endpoints)
        {
            this.transporter = transporter;

            foreach (IPEndPoint endpoint in endpoints)
            {
                peersCompleted[endpoint] = false;
            }
        }

        public override void EndFeed()
        { }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
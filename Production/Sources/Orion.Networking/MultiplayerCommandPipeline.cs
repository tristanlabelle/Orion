using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment;

namespace Orion.Networking
{
    public class MultiplayerCommandPipeline : CommandPipeline
    {
        private CommandSynchronizer synchronizer;
        private CommandLogger logger;

        public override ISinkRecipient AICommandmentEntryPoint
        {
            get { throw new NotImplementedException(); }
        }

        public override ISinkRecipient UserCommandmentEntryPoint
        {
            get { throw new NotImplementedException(); }
        }
    }
}

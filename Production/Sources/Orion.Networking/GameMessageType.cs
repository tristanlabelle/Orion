using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using System.Windows.Forms;

namespace Orion.Networking
{
    public enum GameMessageType : byte
    {
        Commands = 0xC0,
        Done = 0xD0,
    }
}
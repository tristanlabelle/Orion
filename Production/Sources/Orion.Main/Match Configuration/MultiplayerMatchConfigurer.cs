using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Color = System.Drawing.Color;

using Orion.Networking;
using Orion.Graphics;
using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Main
{
    abstract class MultiplayerMatchConfigurer : MatchConfigurer
    {
        protected Transporter transporter;
        protected IEnumerable<IPEndPoint> peers;

        public MultiplayerMatchConfigurer(Transporter transporter)
        {
            this.transporter = transporter;
        }

        public abstract void CreateNetworkConfiguration();
        protected abstract void AssignFactions(out UserInputCommander userCommander);

        public override Match Start()
        {
            CreateMap();
            CommandPipeline pipeline = new MultiplayerCommandPipeline(world, transporter, peers);
            UserInputCommander userCommander;
            AssignFactions(out userCommander);
            userCommander.AddToPipeline(pipeline);
            return new Match(random, terrain, world, userCommander, pipeline);
        }
    }
}

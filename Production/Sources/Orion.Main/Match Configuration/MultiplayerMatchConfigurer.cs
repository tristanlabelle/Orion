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

        public override Match Start()
        {
            CreateMap();

            Faction redFaction = world.CreateFaction("Red", Color.Red);
            UserInputCommander redCommander = new UserInputCommander(redFaction);

            Faction blueFaction = world.CreateFaction("Blue", Color.Cyan);
            DummyAICommander blueCommander = new DummyAICommander(blueFaction, random);

            CommandPipeline pipeline = new MultiplayerCommandPipeline(world, transporter, peers);

            redCommander.AddToPipeline(pipeline);
            blueCommander.AddToPipeline(pipeline);

            return new Match(random, terrain, world, redCommander, pipeline);
        }
    }
}

using System.Collections.Generic;
using System.Net;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.Networking;

namespace Orion.Main
{
    abstract class MultiplayerMatchConfigurer : MatchConfigurer
    {
        protected SafeTransporter transporter;
        protected IEnumerable<Ipv4EndPoint> peerEndPoints;

        public MultiplayerMatchConfigurer(SafeTransporter transporter)
        {
            this.transporter = transporter;
        }

        public abstract void CreateNetworkConfiguration();
        protected abstract void AssignFactions(out UserInputCommander userCommander);

        public override Match Start()
        {
            CreateMap();
            CommandPipeline pipeline = new MultiplayerCommandPipeline(world, transporter, peerEndPoints);
            UserInputCommander userCommander;
            AssignFactions(out userCommander);
            userCommander.AddToPipeline(pipeline);
            return new Match(random, world, userCommander, pipeline);
        }
    }
}

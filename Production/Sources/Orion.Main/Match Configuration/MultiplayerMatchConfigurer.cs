using System.Collections.Generic;
using System.Net;
using Orion.Commandment;
using Orion.Networking;

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
            return new Match(random, world, userCommander, pipeline);
        }
    }
}

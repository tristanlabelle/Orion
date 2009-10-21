using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment;
using Orion.GameLogic;

namespace Orion.Networking
{
    public class MultiplayerCommandPipeline : CommandPipeline
    {
		#region Fields
        private CommandSynchronizer synchronizer;
        private CommandLogger logger;
		
		private Transporter transporter;
		#endregion
		
		#region Constructors
		public MultiplayerCommandPipeline(World world, Transporter transporter, IEnumerable<IPEndPoint> peers)
		{
			Argument.EnsureNotNull(world, "world");
			Argument.EnsureNotNull(transporter, "transporter");
			Argument.EnsureNotNull(peers, "peers");
			
			this.transporter = transporter;
			logger = new CommandLogger(executor);
			synchronizer = new CommandSynchronizer(world, transporter, peers);
			synchronizer.Recipient = logger;
		}
		#endregion

		#region Properties
        public override ICommandSink AICommandmentEntryPoint
        {
            get { return executor; }
        }

        public override ICommandSink UserCommandmentEntryPoint
        {
            get { return synchronizer; }
        }
		#endregion
		
		#region Methods
		public override void Update(float frameDuration)
		{
			base.Update(frameDuration);
			transporter.Poll();
			synchronizer.Update();
		}
		#endregion
    }
}

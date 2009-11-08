using System.Collections.Generic;
using System.Net;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;

namespace Orion.Networking
{
    public class MultiplayerCommandPipeline : CommandPipeline
    {
        #region Fields
        private CommandSynchronizer synchronizer;
        private CommandTextLogger logger;

        private Transporter transporter;
        #endregion

        #region Constructors
        public MultiplayerCommandPipeline(World world, Transporter transporter, IEnumerable<IPEndPoint> peers)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(transporter, "transporter");
            Argument.EnsureNotNull(peers, "peers");

            this.transporter = transporter;
            logger = new CommandTextLogger(executor);
            synchronizer = new CommandSynchronizer(world, transporter, peers);
            synchronizer.Recipient = logger;
        }
        #endregion

        #region Properties
        public override ICommandSink AICommandmentEntryPoint
        {
            get { return logger; }
        }

        public override ICommandSink UserCommandmentEntryPoint
        {
            get { return synchronizer; }
        }
        #endregion

        #region Methods
        public override void Update(int frameNumber, float frameDuration)
        {
            UpdateCommanders(frameDuration);
            transporter.Poll();
            synchronizer.Update(frameNumber);
        }
        #endregion
    }
}

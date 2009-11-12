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

        private SafeTransporter transporter;
        #endregion

        #region Constructors
        public MultiplayerCommandPipeline(World world, SafeTransporter transporter, IEnumerable<Ipv4EndPoint> peerEndPoints)
        {
            Argument.EnsureNotNull(world, "world");
            Argument.EnsureNotNull(transporter, "transporter");
            Argument.EnsureNotNull(peerEndPoints, "peerEndPoints");

            this.transporter = transporter;
            logger = new CommandTextLogger(executor);
            synchronizer = new CommandSynchronizer(world, transporter, peerEndPoints);
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

using System;
using System.Collections.Generic;
using System.Net;
using Color = System.Drawing.Color;
using Orion.GameLogic;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.Networking;
using Orion.UserInterface;

namespace Orion.Main
{
    abstract class MultiplayerMatchConfigurer : MatchConfigurer, IDisposable
    {
        private GenericEventHandler<SafeTransporter, NetworkEventArgs> received;
        private GenericEventHandler<SafeTransporter, IPv4EndPoint> timedOut;
        protected SafeTransporter transporter;

        public MultiplayerMatchConfigurer(SafeTransporter transporter)
        {
            this.transporter = transporter;
            received = Received;
            timedOut = TimedOut;
            transporter.Received += received;
            transporter.TimedOut += timedOut;
        }

        public new MultiplayerMatchConfigurationUI UserInterface
        {
            get { return (MultiplayerMatchConfigurationUI)AbstractUserInterface; }
        }

        protected abstract void TimedOut(SafeTransporter source, IPv4EndPoint host);
        protected abstract void Received(SafeTransporter source, NetworkEventArgs args);
        protected abstract void ExitGame(MatchConfigurationUI ui);

        public override Match Start()
        {
            CreateMap();

            CommandPipeline pipeline = new CommandPipeline();
            pipeline.AddFilter(new CommandReplayLogger("replay.foo", world));
            CommandTextLogger textLogger = new CommandTextLogger();
            pipeline.AddFilter(textLogger);
            CommandSynchronizer synchronizer = new CommandSynchronizer(world, transporter, UserInterface.PlayerAddresses);
            pipeline.AddFilter(synchronizer);

            UserInputCommander userCommander = null;

            int colorIndex = 0;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (slot is ClosedPlayerSlot) continue;
                if (slot is RemotePlayerSlot && !((RemotePlayerSlot)slot).RemoteHost.HasValue) continue;

                Color color = playerColors[colorIndex];
                Faction faction = world.CreateFaction(color.Name, color);
                colorIndex++;

                if (slot is LocalPlayerSlot)
                {
                    userCommander = new UserInputCommander(faction);
                    pipeline.AddCommander(userCommander, synchronizer);
                }
                else if (slot is AIPlayerSlot)
                {
                    Commander commander = new AICommander(faction, random);
                    // AIs bypass the synchronization filter as they are supposed to be fully deterministic
                    pipeline.AddCommander(commander, textLogger);
                }
                else if (!(slot is RemotePlayerSlot)) // no commanders for remote players
                {
                    throw new InvalidOperationException("Multiplayer games only support remote, local and AI players");
                }
            }

            return new Match(random, world, userCommander, pipeline);
        }

        public virtual void Dispose()
        {
            transporter.Received -= received;
            transporter.TimedOut -= timedOut;
        }
    }
}

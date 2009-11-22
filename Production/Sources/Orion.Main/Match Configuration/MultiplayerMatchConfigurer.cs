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

            UserInputCommander userCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
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
                }
                else if (slot is AIPlayerSlot)
                {
                    Commander commander = new AICommander(faction, random);
                    // AIs bypass the synchronization filter as they are supposed to be fully deterministic
                    aiCommanders.Add(commander);
                }
                else if (!(slot is RemotePlayerSlot)) // no commanders for remote players
                {
                    throw new InvalidOperationException("Multiplayer games only support remote, local and AI players");
                }
            }

            Match match = new Match(random, world, userCommander);

            CommandPipeline pipeline = new CommandPipeline(match);
            pipeline.AddFilter(new CommandReplayLogger("replay.foo"));
            CommandTextLogger textLogger = new CommandTextLogger();
            pipeline.AddFilter(textLogger);
            CommandSynchronizer synchronizer = new CommandSynchronizer(world, transporter, UserInterface.PlayerAddresses);
            pipeline.AddFilter(synchronizer);

            aiCommanders.ForEach(commander => pipeline.AddCommander(commander, textLogger));
            pipeline.AddCommander(userCommander, synchronizer);

            match.Updated += pipeline.Update;

            return match;
        }

        public virtual void Dispose()
        {
            transporter.Received -= received;
            transporter.TimedOut -= timedOut;
        }
    }
}

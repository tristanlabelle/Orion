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
        protected SafeTransporter transporter;
        protected List<IPv4EndPoint> peers = new List<IPv4EndPoint>();

        public MultiplayerMatchConfigurer(SafeTransporter transporter)
        {
            this.transporter = transporter;
            received = Received;
            transporter.Received += received;
        }

        public new MultiplayerMatchConfigurationUI UserInterface
        {
            get { return (MultiplayerMatchConfigurationUI)AbstractUserInterface; }
        }

        protected abstract void Received(SafeTransporter source, NetworkEventArgs args);
        protected abstract void ExitGame(MatchConfigurationUI ui);

        public override Match Start()
        {
            CreateMap();

            CommandPipeline pipeline = new MultiplayerCommandPipeline(world, transporter, UserInterface.PlayerAddresses);
            UserInputCommander userCommander = null;

            int colorIndex = 0;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (slot is ClosedPlayerSlot) continue;
                if (slot is RemotePlayerSlot && !((RemotePlayerSlot)slot).RemoteHost.HasValue) continue;

                Commander commander;
                Color color = playerColors[colorIndex];
                Faction faction = world.CreateFaction(color.Name, color);
                colorIndex++;

                if (slot is LocalPlayerSlot)
                {
                    userCommander = new UserInputCommander(faction);
                    commander = userCommander;
                }
                else if (slot is RemotePlayerSlot) continue; // no commanders for remote players
                else if (slot is AIPlayerSlot) commander = new AICommander(faction, random);
                else throw new InvalidOperationException("Multiplayer games only support remote, local and AI players");

                commander.AddToPipeline(pipeline);
            }

            return new Match(random, world, userCommander, pipeline);
        }

        public virtual void Dispose()
        {
            transporter.Received -= received;
        }
    }
}

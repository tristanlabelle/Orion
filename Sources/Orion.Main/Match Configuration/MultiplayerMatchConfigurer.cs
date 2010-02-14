using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Orion.Commandment;
using Orion.Commandment.Commands.Pipeline;
using Orion.GameLogic;
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

        public override void Start(out Match match, out SlaveCommander localCommander)
        {
            CreateWorld(UserInterface.MapSize);

            localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            List<FactionEndPoint> peers = new List<FactionEndPoint>();
            int colorIndex = 0;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (slot is ClosedPlayerSlot) continue;
                if (slot is RemotePlayerSlot && !((RemotePlayerSlot)slot).RemoteHost.HasValue) continue;

                ColorRgb color = Faction.Colors[colorIndex];
                Faction faction = world.CreateFaction(Colors.GetName(color), color);
                colorIndex++;

                if (slot is LocalPlayerSlot)
                {
                    localCommander = new SlaveCommander(faction);
                }
                else if (slot is AIPlayerSlot)
                {
                    Commander commander = new AgressiveAICommander(faction, random);
                    // AIs bypass the synchronization filter as they are supposed to be fully deterministic
                    aiCommanders.Add(commander);
                }
                else if (slot is RemotePlayerSlot) // no commanders for remote players
                {
                    IPv4EndPoint endPoint = ((RemotePlayerSlot)slot).RemoteHost.Value;
                    FactionEndPoint peer = new FactionEndPoint(transporter, faction, endPoint);
                    peers.Add(peer);
                }
                else
                {
                    throw new InvalidOperationException("Multiplayer games only support remote, local and AI players");
                }
            }

            match = new Match(random, world);

            CommandPipeline pipeline = new CommandPipeline(match);
            TryPushReplayRecorderToPipeline(pipeline);
            ICommandSink aiCommandSink = pipeline.TopMostSink;
            pipeline.PushFilter(new CommandOptimizer());
            pipeline.PushFilter(new CommandSynchronizer(match, transporter, peers));

            aiCommanders.ForEach(commander => pipeline.AddCommander(commander, aiCommandSink));
            pipeline.AddCommander(localCommander);

            match.Updated += (sender, args) =>
                pipeline.Update(sender.LastSimulationStepNumber, args.TimeDeltaInSeconds);
        }

        public virtual void Dispose()
        {
            transporter.Received -= received;
            transporter.TimedOut -= timedOut;
        }
    }
}

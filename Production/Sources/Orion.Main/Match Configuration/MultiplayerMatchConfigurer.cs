﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Orion.Commandment;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using Orion.Networking;
using Orion.UserInterface;
using Color = System.Drawing.Color;

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
#if false
            RegularStart(out match, out localCommander);
#else
            CommandSynchronizer2Start(out match, out localCommander);
#endif
        }

        private void RegularStart(out Match match, out SlaveCommander localCommander)
        {
            CreateWorld();

            localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            List<PeerEndPoint> peers = new List<PeerEndPoint>();
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
                    localCommander = new SlaveCommander(faction);
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

            match = new Match(random, world);

            CommandPipeline pipeline = new CommandPipeline(match);
            TryPushReplayRecorderToPipeline(pipeline);
            ICommandSink aiCommandSink = pipeline.TopMostSink;
            pipeline.PushFilter(new CommandSynchronizer(match, transporter, UserInterface.PlayerAddresses));

            aiCommanders.ForEach(commander => pipeline.AddCommander(commander, aiCommandSink));
            pipeline.AddCommander(localCommander);

            match.Updated += (sender, args) => pipeline.Update(sender.LastFrameNumber, args.TimeDeltaInSeconds);
        }

        private void CommandSynchronizer2Start(out Match match, out SlaveCommander localCommander)
        {
            CreateWorld();

            localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            List<PeerEndPoint> peers = new List<PeerEndPoint>();
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
                    localCommander = new SlaveCommander(faction);
                }
                else if (slot is AIPlayerSlot)
                {
                    Commander commander = new AICommander(faction, random);
                    // AIs bypass the synchronization filter as they are supposed to be fully deterministic
                    aiCommanders.Add(commander);
                }
                else if (slot is RemotePlayerSlot) // no commanders for remote players
                {
                    IPv4EndPoint endPoint = ((RemotePlayerSlot)slot).RemoteHost.Value;
                    PeerEndPoint peer = new PeerEndPoint(transporter, faction, endPoint);
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
            pipeline.PushFilter(new CommandSynchronizer2(match, transporter, peers));

            aiCommanders.ForEach(commander => pipeline.AddCommander(commander, aiCommandSink));
            pipeline.AddCommander(localCommander);

            match.Updated += (sender, args) => pipeline.Update(sender.LastFrameNumber, args.TimeDeltaInSeconds);
        }

        public virtual void Dispose()
        {
            transporter.Received -= received;
            transporter.TimedOut -= timedOut;
        }
    }
}

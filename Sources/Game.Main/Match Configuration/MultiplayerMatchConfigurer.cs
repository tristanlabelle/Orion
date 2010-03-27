﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Orion.Engine;
using Orion.Engine.Networking;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Deathmatch;
using Orion.Game.Matchmaking.Commands.Pipeline;
using Orion.Game.Matchmaking.Networking;
using Orion.Game.Presentation;

namespace Orion.Main
{
    abstract class MultiplayerMatchConfigurer : MatchConfigurer, IDisposable
    {
        private Action<SafeTransporter, NetworkEventArgs> received;
        private Action<SafeTransporter, IPv4EndPoint> timedOut;
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
            CreateWorld(options.MapSize);

            localCommander = null;
            List<Commander> aiCommanders = new List<Commander>();
            List<FactionEndPoint> peers = new List<FactionEndPoint>();
            int colorIndex = 0;
            foreach (PlayerSlot slot in UserInterface.Players)
            {
                if (!slot.NeedsFaction) continue;

                ColorRgb color = Faction.Colors[colorIndex];
                colorIndex++;

                if (slot is LocalPlayerSlot)
                {
                    string hostName = Environment.MachineName;
                    Faction faction = world.CreateFaction(hostName, color, options.InitialAladdiumAmount, options.InitialAlageneAmount);
                    localCommander = new SlaveCommander(faction);
                }
                else if (slot is AIPlayerSlot)
                {
                    Faction faction = world.CreateFaction(Colors.GetName(color), color, options.InitialAladdiumAmount, options.InitialAlageneAmount);
                    Commander commander = new AgressiveAICommander(faction, random);
                    // AIs bypass the synchronization filter as they are supposed to be fully deterministic
                    aiCommanders.Add(commander);
                }
                else if (slot is RemotePlayerSlot) // no commanders for remote players
                {
                    RemotePlayerSlot remotePlayerSlot = (RemotePlayerSlot)slot;
                    IPv4EndPoint endPoint = remotePlayerSlot.HostEndPoint.Value;
                    Faction faction = world.CreateFaction(remotePlayerSlot.ToString(), color, options.InitialAladdiumAmount, options.InitialAlageneAmount);
                    FactionEndPoint peer = new FactionEndPoint(transporter, faction, endPoint);
                    peers.Add(peer);
                }
                else
                {
                    throw new InvalidOperationException("Multiplayer games only support remote, local and AI players");
                }
            }

            WorldGenerator.Generate(world, random);
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

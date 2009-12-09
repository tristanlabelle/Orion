﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.GameLogic;
using Orion.Commandment;
using Orion.Commandment.Commands;
using Orion.Commandment.Commands.Pipeline;
using System.Diagnostics;

namespace Orion.Networking
{
    public class CommandSynchronizer : CommandFilter
    {
        #region Fields
        #region Updates Rate Management
        private const int retainedSeconds = 5;
        private const int updatesPerSecond = 40;
        private const int defaultUpdatesPerFrame = 6;
        private readonly List<int> updatesForCommandFrame = new List<int>();
        private readonly Queue<int> previousFramesDuration = new Queue<int>();
        private int updatesSinceLastCommandFrame = defaultUpdatesPerFrame - 1;
        private int commandFrameNumber = 0;
        #endregion

        #region Peers Handling
        private readonly List<PeerEndPoint> peers;
        private readonly SafeTransporter transporter;
        #endregion

        #region Commands
        private readonly Match match;
        private readonly List<Command> localCommands = new List<Command>();
        private readonly List<Command> commandsToSend = new List<Command>();
        private readonly List<Command> commandsToExecute = new List<Command>();
        #endregion
        #endregion

        #region Constructors
        public CommandSynchronizer(Match match, SafeTransporter transporter, IEnumerable<PeerEndPoint> endPoints)
        {
            Argument.EnsureNotNull(endPoints, "endPoints");
            Argument.EnsureNotNull(transporter, "transporter");

            this.transporter = transporter;
            this.match = match;
            match.Quitting += LeaveGame;
            peers = endPoints.ToList();
            previousFramesDuration.Enqueue(defaultUpdatesPerFrame);

            foreach (Faction faction in match.World.Factions)
            {
                faction.Defeated += FactionDefeated;
            }
        }
        #endregion

        #region Properties
        private bool ReceivedFromAllPeers
        {
            get { return peers.All(peer => peer.HasCommandsForCommandFrame(commandFrameNumber)); }
        }

        private bool AllPeersDone
        {
            get { return peers.All(peer => peer.IsDoneForFrame(commandFrameNumber)); }
        }

        private int TargetUpdatesPerCommandFrame
        {
            get
            {
                int average = (int)(previousFramesDuration.Average() + 0.5);
                int deviation = (int)Math.Sqrt(previousFramesDuration.Select(i => (i - average) * (i - average)).Average());
                return average + deviation * 2;
            }
        }
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            localCommands.Add(command);
        }

        public override void Update(int updateNumber, float timeDeltaInSeconds)
        {
            transporter.Poll();
            updatesSinceLastCommandFrame++;

            if (updatesSinceLastCommandFrame == TargetUpdatesPerCommandFrame)
            {
                peers.ForEach(peer => peer.SendCommands(commandFrameNumber, commandsToSend));
                commandsToExecute.AddRange(commandsToSend);
                commandsToSend.Clear();
                commandsToSend.AddRange(localCommands);
                localCommands.Clear();
            }

            if (ReceivedFromAllPeers && updatesForCommandFrame.Count == 0)
            {
                updatesForCommandFrame.Add(updatesSinceLastCommandFrame);
                peers.ForEach(peer => peer.SendDone(commandFrameNumber, updatesSinceLastCommandFrame));
            }

            if (updatesSinceLastCommandFrame >= TargetUpdatesPerCommandFrame * 2)
            {
                if (!AllPeersDone || !ReceivedFromAllPeers)
                {
                    match.Pause();
                    return;
                }
                match.Resume();

                AdaptUpdatesPerCommandFrame();
                peers.ForEach(peer => commandsToExecute.AddRange(peer.GetCommandsForCommandFrame(commandFrameNumber)));
                FlushCommands();

                commandFrameNumber++;
                updatesSinceLastCommandFrame = 0;
            }
        }

        private void AdaptUpdatesPerCommandFrame()
        {
            updatesForCommandFrame.AddRange(peers.Select(peer => peer.GetUpdatesForCommandFrame(commandFrameNumber)));
            int longestCommandFrame = updatesForCommandFrame.Max() - TargetUpdatesPerCommandFrame;
            string[] updates = updatesForCommandFrame.OrderBy(i => i).Select(i => i.ToString()).ToArray();
            Debug.WriteLine("Current UPCFs are {{{0}}}, selected={1}".FormatInvariant(string.Join(", ", updates), longestCommandFrame));
            
            previousFramesDuration.Enqueue(longestCommandFrame);
            while (previousFramesDuration.Count > 1 && previousFramesDuration.Sum() > retainedSeconds * updatesPerSecond)
                previousFramesDuration.Dequeue();
            updatesForCommandFrame.Clear();
        }

        private void FlushCommands()
        {
            foreach (Command command in commandsToExecute.OrderBy(c => c.FactionHandle.Value))
                Flush(command);
            commandsToExecute.Clear();
        }

        private void FactionDefeated(Faction sender)
        {
            PeerEndPoint associatedEndPoint = peers.FirstOrDefault(peer => peer.Faction == sender);
            if (associatedEndPoint != null)
            {
                associatedEndPoint.Dispose();
                peers.Remove(associatedEndPoint);
            }
        }

        private void LeaveGame(Match sender)
        {
            peers.ForEach(peer => peer.SendLeave());
        }
        #endregion
    }
}

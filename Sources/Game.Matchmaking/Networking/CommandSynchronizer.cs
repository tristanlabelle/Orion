using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Networking;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands;
using Orion.Game.Matchmaking.Commands.Pipeline;

namespace Orion.Game.Matchmaking.Networking
{
    /// <summary>
    /// A command filter which ensures that commands coming in
    /// are synchronized and executed simultaneously on every host in the game.
    /// </summary>
    public sealed class CommandSynchronizer : CommandFilter
    {
        #region Fields
        private const int retentionDelay = 4;
        private const int updatesPerSecond = 40;
        private const int defaultUpdatesPerFrame = 6;
        private const int maxUpdatesPerFrame = 10;

        private readonly Match match;
        private readonly GameNetworking networking;
        private readonly List<FactionEndPoint> peers;

        private readonly List<int> updatesForCommandFrame = new List<int>();
        private readonly Queue<int> previousFramesDuration = new Queue<int>();

        private readonly List<Command> localCommands = new List<Command>();
        private readonly List<Command> commandsToSend = new List<Command>();
        private readonly List<Command> commandsToExecute = new List<Command>();

        private int updatesSinceLastCommandFrame = defaultUpdatesPerFrame - 1;
        private int commandFrameNumber = 0;
        #endregion

        #region Constructors
        public CommandSynchronizer(Match match, GameNetworking networking,
            IEnumerable<FactionEndPoint> endPoints)
        {
            Argument.EnsureNotNull(endPoints, "endPoints");
            Argument.EnsureNotNull(networking, "networking");

            this.networking = networking;
            this.match = match;
            peers = endPoints.ToList();
            previousFramesDuration.Enqueue(defaultUpdatesPerFrame);

            foreach (Faction faction in match.World.Factions)
                faction.Defeated += FactionDefeated;
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
                // adaptative frame rate
                int average = (int)(previousFramesDuration.Average() + 0.5);
                int deviation = (int)Math.Sqrt(previousFramesDuration.Select(i => (i - average) * (i - average)).Average());
                return Math.Min(average + deviation * 2, maxUpdatesPerFrame);
            }
        }
        #endregion

        #region Methods
        public override void Handle(Command command)
        {
            Argument.EnsureNotNull(command, "command");
            localCommands.Add(command);
        }

        public override void Update(SimulationStep step)
        {
            networking.Poll();
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
                    Debug.WriteLine("Match paused!", "Network");
                    match.Pause();
                    return;
                }
                match.Resume();
                
                var commands = peers.SelectMany(peer => peer.GetCommandsForCommandFrame(commandFrameNumber));
                commandsToExecute.AddRange(commands);

                AdaptUpdatesPerCommandFrame();
                FlushCommands();

                commandFrameNumber++;
                updatesSinceLastCommandFrame = 0;
            }
        }

        public override void Dispose()
        {
            peers.ForEach(peer => peer.SendLeave());
        }

        private void AdaptUpdatesPerCommandFrame()
        {
            updatesForCommandFrame.AddRange(peers.Select(peer => peer.GetUpdatesForCommandFrame(commandFrameNumber)));
            int longestCommandFrame = updatesForCommandFrame.Max() - TargetUpdatesPerCommandFrame;
            string[] updates = updatesForCommandFrame.OrderBy(i => i).Select(i => i.ToString()).ToArray();
            
            previousFramesDuration.Enqueue(longestCommandFrame);
            while (previousFramesDuration.Count > 1 && previousFramesDuration.Sum() > retentionDelay * updatesPerSecond)
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
            FactionEndPoint associatedEndPoint = peers.FirstOrDefault(peer => peer.Faction == sender);
            if (associatedEndPoint != null)
            {
                associatedEndPoint.Dispose();
                peers.Remove(associatedEndPoint);
            }
        }
        #endregion
    }
}

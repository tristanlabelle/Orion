using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Orion.Commandment;
using Orion.Commandment.Pipeline;
using System.Diagnostics;

namespace Orion.Networking
{
    public class CommandSynchronizer2 : CommandFilter
    {
        #region Fields
        #region Frame Rate Management
        private const int defaultUpdatesPerFrame = 6;
        private const int maxEnqueuedFrameDurations = 0x48;
        private readonly List<int> updatesForCommandFrame = new List<int>();
        private readonly Queue<int> previousFramesDuration = new Queue<int>();
        private int updatesSinceLastCommandFrame = 0;
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
        private bool needsToSendCommands;
        #endregion
        #endregion

        #region Constructors
        public CommandSynchronizer2(Match match, SafeTransporter transporter, IEnumerable<PeerEndPoint> endPoints)
        {
            Argument.EnsureNotNull(endPoints, "endPoints");
            Argument.EnsureNotNull(transporter, "transporter");

            this.transporter = transporter;
            this.match = match;
            peers = endPoints.ToList();
            previousFramesDuration.Enqueue(defaultUpdatesPerFrame);
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
            get { return (int)(previousFramesDuration.Average() + 0.5); }
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
            updatesSinceLastCommandFrame++;

            if (ReceivedFromAllPeers && updatesForCommandFrame.Count == 0)
            {
                updatesForCommandFrame.Add(updatesSinceLastCommandFrame);
                peers.ForEach(peer => peer.SendDone(commandFrameNumber, updatesSinceLastCommandFrame));
            }

            if (updatesSinceLastCommandFrame == TargetUpdatesPerCommandFrame)
            {
                peers.ForEach(peer => peer.SendCommands(commandFrameNumber, commandsToSend));
                commandsToExecute.AddRange(commandsToSend);
                commandsToSend.Clear();
                commandsToSend.AddRange(localCommands);
                localCommands.Clear();
            }

            if (updatesSinceLastCommandFrame >= TargetUpdatesPerCommandFrame)
            {
                if (!AllPeersDone || ReceivedFromAllPeers)
                {
                    Debug.WriteLine("Waiting for peers");
                    match.Pause();
                    return;
                }
                match.Resume();

                AdaptUpdatesPerCommandFrame();
                FlushCommands();
            }
        }

        private void AdaptUpdatesPerCommandFrame()
        {
            updatesForCommandFrame.AddRange(peers.Select(peer => peer.GetUpdatesForCommandFrame(commandFrameNumber)));
            previousFramesDuration.Enqueue(updatesForCommandFrame.Max());
            if (previousFramesDuration.Count > maxEnqueuedFrameDurations)
                previousFramesDuration.Dequeue();
            updatesForCommandFrame.Clear();
        }

        private void FlushCommands()
        {
            foreach (Command command in commandsToExecute.OrderBy(c => c.FactionHandle.Value))
                Flush(command);
            commandsToExecute.Clear();
        }
        #endregion
    }
}

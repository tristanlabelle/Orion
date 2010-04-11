using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Orion.Engine;
using Orion.Engine.Networking;
using Orion.Game.Simulation;
using Orion.Game.Matchmaking;
using Orion.Game.Matchmaking.Commands;
using Orion.Engine.Collections;

namespace Orion.Game.Matchmaking.Networking
{
    public class FactionEndPoint : IDisposable
    {
        #region Fields
        private readonly byte[] doneMessage;

        private Action<SafeTransporter, NetworkEventArgs> receive;
        private Action<SafeTransporter, IPv4EndPoint> timeout;
        private readonly SafeTransporter transporter;

        private readonly Dictionary<int, List<Command>> availableCommands = new Dictionary<int, List<Command>>();
        private readonly Dictionary<int, int> updatesForDone = new Dictionary<int, int>();

        public readonly IPv4EndPoint Host;
        public readonly Faction Faction;
        #endregion

        #region Constructors
        public FactionEndPoint(SafeTransporter transporter, Faction faction, IPv4EndPoint host)
        {
            this.transporter = transporter;
            receive = OnReceived;
            timeout = OnTimedOut;

            transporter.Received += receive;
            transporter.TimedOut += timeout;
            Faction = faction;
            Host = host;

            availableCommands[0] = new List<Command>();
            doneMessage = new byte[1 + sizeof(int) * 2];
            doneMessage[0] = (byte)GameMessageType.Done;
        }
        #endregion

        #region Methods
        public bool IsDoneForFrame(int commandFrame)
        {
            return updatesForDone.ContainsKey(commandFrame);
        }

        public int GetUpdatesForCommandFrame(int commandFrame)
        {
            return updatesForDone[commandFrame];
        }

        public bool HasCommandsForCommandFrame(int commandFrame)
        {
            return availableCommands.ContainsKey(commandFrame);
        }

        public List<Command> GetCommandsForCommandFrame(int commandFrame)
        {
            if (!HasCommandsForCommandFrame(commandFrame))
                throw new InvalidOperationException("Did not receive commands for frame {0}".FormatInvariant(commandFrame));

            return availableCommands[commandFrame];
        }

        public void SendLeave()
        {
            byte[] quitMessage = new byte[1];
            quitMessage[0] = (byte)GameMessageType.Quit;
            transporter.SendTo(quitMessage, Host);
        }

        public void SendDone(int commandFrame, int numberOfUpdates)
        {
            BitConverter.GetBytes(commandFrame).CopyTo(doneMessage, 1);
            BitConverter.GetBytes(numberOfUpdates).CopyTo(doneMessage, 1 + sizeof(int));
            transporter.SendTo(doneMessage, Host);
        }

        private List<Command> DeserializeCommandDatagram(byte[] data, int startIndex)
        {
            Subarray<byte> subarray = new Subarray<byte>(data, startIndex);
            List<Command> commands = Command.Serializer.DeserializeToEnd(subarray);

            Command firstMindControlledCommand = commands
                .FirstOrDefault(c => c.FactionHandle != Faction.Handle);
            if (firstMindControlledCommand != null)
            {
                Debug.Fail("Faction {0} is mind controlling faction {1}."
                    .FormatInvariant(Faction.Handle, firstMindControlledCommand.FactionHandle));
            }

            return commands;
        }

        public void SendCommands(int commandFrame, IEnumerable<Command> commands)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream, Encoding.UTF8);

            writer.Write((byte)GameMessageType.Commands);
            writer.Write(commandFrame);
            foreach (Command command in commands)
                Command.Serializer.Serialize(command, writer);

            writer.Flush();
            transporter.SendTo(stream.ToArray(), Host);
        }

        private void OnReceived(SafeTransporter transporter, NetworkEventArgs args)
        {
            if (args.Host != Host) return;

            if (args.Data[0] == (byte)GameMessageType.Quit)
                Faction.MassSuicide();
            else
            {
                if (args.Data[0] == (byte)GameMessageType.Commands)
                {
                    int commandFrame = BitConverter.ToInt32(args.Data, 1);
                    availableCommands[commandFrame] = DeserializeCommandDatagram(args.Data, 1 + sizeof(int));
                }
                else if (args.Data[0] == (byte)GameMessageType.Done)
                {
                    int commandFrame = BitConverter.ToInt32(args.Data, 1);
                    int updates = BitConverter.ToInt32(args.Data, 1 + sizeof(int));
                    updatesForDone[commandFrame] = updates;
                }
            }
        }

        private void OnTimedOut(SafeTransporter transporter, IPv4EndPoint endPoint)
        {
            if (endPoint != Host) return;

            Faction.RaiseWarning("Perdu la connexion à {0}.".FormatInvariant(endPoint));
            Faction.MarkAsDefeated();
        }

        public void Dispose()
        {
            transporter.Received -= receive;
            transporter.TimedOut -= timeout;
        }
        #endregion
    }
}

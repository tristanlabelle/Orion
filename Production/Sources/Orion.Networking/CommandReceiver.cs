using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Networking
{
    public class CommandReceiver : IDisposable
    {
        #region Fields
        private GenericEventHandler<SafeTransporter, NetworkEventArgs> receive;
        private GenericEventHandler<SafeTransporter, IPv4EndPoint> timeout;
        private readonly SafeTransporter transporter;

        private readonly Dictionary<int, List<Command>> availableCommands = new Dictionary<int, List<Command>>();
        private int lastDoneReceived = 0;

        public readonly Faction Faction;
        public readonly IPv4EndPoint Host;
        #endregion

        #region Constructors
        public CommandReceiver(SafeTransporter transporter, Faction faction, IPv4EndPoint host)
        {
            receive = OnReceived;
            timeout = OnTimedOut;
            this.transporter = transporter;
            Faction = faction;
            Host = host;
        }
        #endregion

        #region Events
        public event GenericEventHandler<CommandReceiver> HostLeftGame;
        #endregion

        #region Methods
        public bool IsDoneForFrame(int commandFrame)
        {
            return commandFrame <= lastDoneReceived;
        }

        public bool HasCommandsForCommandFrame(int commandFrame)
        {
            return availableCommands.ContainsKey(commandFrame);
        }

        private List<Command> GetCommandsForCommandFrame(int commandFrame)
        {
            if (!HasCommandsForCommandFrame(commandFrame))
                throw new InvalidOperationException("Did not receive commands for frame {0}".FormatInvariant(commandFrame));

            return availableCommands[commandFrame];
        }

        private void OnReceived(SafeTransporter transporter, NetworkEventArgs args)
        {
            if (args.Host != Host) return;

            if (args.Data[0] == (byte)GameMessageType.Quit)
            {
                
            }
            else
            {
                int commandFrame = BitConverter.ToInt32(args.Data, 1);
                if (args.Data[0] == (byte)GameMessageType.Commands)
                {
                    availableCommands[commandFrame] = DeserializeCommandDatagram(args.Data, 1 + sizeof(int));
                    // remove older entries
                    if (HasCommandsForCommandFrame(commandFrame - 2))
                        availableCommands.Remove(commandFrame - 2);
                }
                else if (args.Data[0] == (byte)GameMessageType.Done)
                {
                    lastDoneReceived = commandFrame;
                }
            }
        }

        private void OnTimedOut(SafeTransporter transporter, IPv4EndPoint endPoint)
        {
            if (endPoint != Host) return;
            OnLeaveGame();
        }

        private void OnLeaveGame()
        {
            if (HostLeftGame != null) HostLeftGame(this);
        }

        private List<Command> DeserializeCommandDatagram(byte[] data, int startIndex)
        {
            using (MemoryStream stream = new MemoryStream(data, startIndex, data.Length - startIndex))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int commandFrame = reader.ReadInt32();
                    List<Command> commands = new List<Command>();
                    while (stream.Position != stream.Length)
                    {
                        Command deserializedCommand = Command.Deserialize(reader);
                        commands.Add(deserializedCommand);
                    }

                    return commands;
                }
            }
        }

        public void Dispose()
        {

        }
        #endregion
    }
}

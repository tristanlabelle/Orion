using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Orion.GameLogic;
using Orion.Commandment;
using Orion.Commandment.Commands;
using System.Diagnostics;

namespace Orion.Networking
{
    public class FactionEndPoint : IDisposable
    {
        #region Fields
        private readonly byte[] doneMessage;

        private GenericEventHandler<SafeTransporter, NetworkEventArgs> receive;
        private GenericEventHandler<SafeTransporter, IPv4EndPoint> timeout;
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
            using (MemoryStream stream = new MemoryStream(data, startIndex, data.Length - startIndex))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    List<Command> commands = new List<Command>();
                    while (stream.Position != stream.Length)
                    {
                        Command deserializedCommand = Command.Deserialize(reader);

#if DEBUG
                        // #if'd so FormatInvariant is not executed in release
                        Debug.Assert(deserializedCommand.FactionHandle == Faction.Handle,
                            "Faction #{0} attempted mind control".FormatInvariant(deserializedCommand.FactionHandle));
#endif

                        commands.Add(deserializedCommand);
                    }
                    return commands;
                }
            }
        }

        public void SendCommands(int commandFrame, IEnumerable<Command> commands)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)GameMessageType.Commands);
                    writer.Write(commandFrame);
                    foreach (Command command in commands)
                        command.Serialize(writer);
                }
                transporter.SendTo(stream.ToArray(), Host);
            }
        }

        private void OnReceived(SafeTransporter transporter, NetworkEventArgs args)
        {
            if (args.Host != Host) return;

            if (args.Data[0] == (byte)GameMessageType.Quit)
                Faction.GiveUp();
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
            Faction.RaiseWarning("Perdu la connexion à {0}".FormatInvariant(endPoint));
            Faction.GiveUp();
        }

        public void Dispose()
        {
            transporter.Received -= receive;
            transporter.TimedOut -= timeout;
        }
        #endregion
    }
}

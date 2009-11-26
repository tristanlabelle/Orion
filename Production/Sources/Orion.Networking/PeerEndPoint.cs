﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Orion.GameLogic;
using Orion.Commandment;

namespace Orion.Networking
{
    public class PeerEndPoint : IDisposable
    {
        #region Fields
        private readonly byte[] doneMessage = new byte[] { (byte)GameMessageType.Done, 0, 0, 0, 0 };

        private GenericEventHandler<SafeTransporter, NetworkEventArgs> receive;
        private GenericEventHandler<SafeTransporter, IPv4EndPoint> timeout;
        private readonly SafeTransporter transporter;

        private readonly Dictionary<int, List<Command>> availableCommands = new Dictionary<int, List<Command>>();
        private readonly Dictionary<int, int> updatesForDone = new Dictionary<int, int>();

        public readonly IPv4EndPoint Host;
        public readonly Faction Faction;
        #endregion

        #region Constructors
        public PeerEndPoint(SafeTransporter transporter, Faction faction, IPv4EndPoint host)
        {
            this.transporter = transporter;
            receive = OnReceived;
            timeout = OnTimedOut;

            transporter.Received += receive;
            transporter.TimedOut += timeout;
            Faction = faction;
            Host = host;

            availableCommands[0] = new List<Command>();
            updatesForDone[0] = 6;
        }
        #endregion

        #region Events
        public event GenericEventHandler<PeerEndPoint> LeftGame;
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

        public void SendDone(int commandFrame)
        {
            BitConverter.GetBytes(commandFrame).CopyTo(doneMessage, 1);
            transporter.SendTo(doneMessage, Host);
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
                OnLeaveGame();
            else
            {
                int commandFrame = BitConverter.ToInt32(args.Data, 1);
                if (args.Data[0] == (byte)GameMessageType.Commands)
                {
                    availableCommands[commandFrame] = DeserializeCommandDatagram(args.Data, 1 + sizeof(int));
                }
                else if (args.Data[0] == (byte)GameMessageType.Done)
                {
                    int updates = BitConverter.ToInt32(args.Data, 1);
                    updatesForDone[commandFrame] = updates;
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
            if (LeftGame != null) LeftGame(this);
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
            transporter.Received -= receive;
            transporter.TimedOut -= timeout;
        }
        #endregion
    }
}

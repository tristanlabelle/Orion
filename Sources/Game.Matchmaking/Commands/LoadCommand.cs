using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Tasks;
using Orion.Game.Simulation.Components;

namespace Orion.Game.Matchmaking.Commands
{
    public sealed class LoadCommand : Command
    {
        #region Fields
        private readonly Handle transporter;
        private readonly Handle transportee;
        #endregion

        #region Constructors
        public LoadCommand(Handle factionHandle, Handle transporter, Handle transportee)
            : base(factionHandle)
        {
            this.transporter = transporter;
            this.transportee = transportee;
        }
        #endregion

        #region Properties
        public override bool IsMandatory
        {
            get { return false; }
        }

        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { yield return transporter; yield return transportee; }
        }

        public Handle TransporterHandle
        {
            get { return transporter; }
        }

        public Handle TransporteeHandle
        {
            get { return transportee; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && IsValidEntityHandle(match, transporter)
                && IsValidEntityHandle(match, transportee);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Entity transporter = match.World.Entities.FromHandle(this.transporter);
            Entity transportee = match.World.Entities.FromHandle(this.transportee);
            transporter.Components.Get<TaskQueue>().Enqueue(new LoadTask(transporter, transportee));
            transportee.Components.Get<TaskQueue>().Enqueue(new FollowTask(transportee, transporter));
        }

        public override string ToString()
        {
            return "Faction {0} loads {1} with {2}"
                .FormatInvariant(FactionHandle, transportee, transporter);
        }

        #region Serialization
        public static void Serialize(LoadCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteHandle(writer, command.TransporterHandle);
            WriteHandle(writer, command.TransporteeHandle);
        }

        public static LoadCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            Handle transporterHandle = ReadHandle(reader);
            Handle transporteeHandle = ReadHandle(reader);
            return new LoadCommand(factionHandle, transporterHandle, transporteeHandle);
        }
        #endregion
        #endregion
    }
}
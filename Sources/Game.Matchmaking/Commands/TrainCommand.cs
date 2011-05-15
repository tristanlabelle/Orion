using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Orion.Engine;
using Orion.Engine.Collections;
using Orion.Game.Simulation;
using Orion.Game.Simulation.Components;
using Orion.Game.Simulation.Tasks;

namespace Orion.Game.Matchmaking.Commands
{
    /// <summary>
    /// A <see cref="Command"/> which causes the <see cref="TrainTask"/> task
    /// to be assigned to some <see cref="Entity"/>s.
    /// </summary>
    public sealed class TrainCommand : Command
    {
        #region Fields
        private readonly ReadOnlyCollection<Handle> trainerHandles;
        private readonly Handle traineePrototypeHandle;
        private readonly int traineeCount;
        #endregion

        #region Constructors
        public TrainCommand(Handle factionHandle, IEnumerable<Handle> trainerHandles, Handle traineePrototypeHandle, int traineeCount)
            : base(factionHandle)
        {
            Argument.EnsureNotNull(trainerHandles, "trainerHandles");
            Argument.EnsureStrictlyPositive(traineeCount, "traineeCount");
            Debug.Assert(traineeCount <= byte.MaxValue);

            this.trainerHandles = trainerHandles.Distinct().ToList().AsReadOnly();
            this.traineePrototypeHandle = traineePrototypeHandle;
            this.traineeCount = traineeCount;
        }

        public TrainCommand(Handle factionHandle, Handle trainerHandle, Handle traineePrototypeHandle, int traineeCount)
            : this(factionHandle, new[] { trainerHandle }, traineePrototypeHandle, traineeCount) { }

        public TrainCommand(Handle factionHandle, IEnumerable<Handle> trainerHandles, Handle traineePrototypeHandle)
            : this(factionHandle, trainerHandles, traineePrototypeHandle, 1) { }

        public TrainCommand(Handle factionHandle, Handle trainerHandle, Handle traineePrototypeHandle)
            : this(factionHandle, trainerHandle, traineePrototypeHandle, 1) { }
        #endregion

        #region Properties
        public override IEnumerable<Handle> ExecutingUnitHandles
        {
            get { return trainerHandles; }
        }

        public Handle TraineeTypeHandle
        {
            get { return traineePrototypeHandle; }
        }

        /// <summary>
        /// Gets the number of trainees to be queued for training.
        /// </summary>
        public int TraineeCount
        {
            get { return traineeCount; }
        }
        #endregion

        #region Methods
        public override bool ValidateHandles(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            return IsValidFactionHandle(match, FactionHandle)
                && trainerHandles.All(handle => IsValidEntityHandle(match, handle))
                && IsValidUnitTypeHandle(match, traineePrototypeHandle);
        }

        public override void Execute(Match match)
        {
            Argument.EnsureNotNull(match, "match");

            Faction faction = match.World.FindFactionFromHandle(FactionHandle);

            Entity traineePrototype = match.Prototypes.FromHandle(traineePrototypeHandle);
            ResourceAmount cost = ResourceAmount.FromEntityCost(traineePrototype, faction);

            for (int i = 0; i < traineeCount; ++i)
            {
                // Find the trainer with the least amount of queued tasks.
                TaskQueue mostFreeTaskQueue = null;
                foreach (Handle trainerHandle in trainerHandles)
                {
                    Entity trainer = match.World.Entities.FromHandle(trainerHandle);
                    TaskQueue taskQueue = trainer.Components.TryGet<TaskQueue>();
                    if (trainer.Components.Has<Trainer>()
                        && taskQueue != null
                        && (mostFreeTaskQueue == null || taskQueue.Count < mostFreeTaskQueue.Count))
                    {
                        mostFreeTaskQueue = taskQueue;
                    }
                }

                if (mostFreeTaskQueue == null) break;

                if (cost.Alagene > faction.AlageneAmount || cost.Aladdium > faction.AladdiumAmount)
                {
                    faction.RaiseWarning("Pas assez de ressources pour entraîner un {0}."
                        .FormatInvariant(traineePrototype.Identity.Name));
                    break;
                }

                if (cost.Food > faction.RemainingFoodAmount)
                {
                    faction.RaiseWarning("Pas assez de nourriture pour entraîner un {0}."
                        .FormatInvariant(traineePrototype.Identity.Name));
                    break;
                }

                faction.AlageneAmount -= cost.Alagene;
                faction.AladdiumAmount -= cost.Aladdium;

                // The hero randomization must be done here to so that every individual train
                // an be a hero or not. Otherwise, heroes are created on all training units.
                var actualTraineeType = match.RandomizeHero(traineePrototype);
                mostFreeTaskQueue.Enqueue(new TrainTask(mostFreeTaskQueue.Entity, actualTraineeType));
            }
        }

        public override string ToString()
        {
            return "Faction {0} trains {1} {2} with {3}"
                .FormatInvariant(FactionHandle, traineeCount, traineePrototypeHandle,
                trainerHandles.ToCommaSeparatedValues());
        }
        
        #region Serialization
        public static void Serialize(TrainCommand command, BinaryWriter writer)
        {
            Argument.EnsureNotNull(command, "command");
            Argument.EnsureNotNull(writer, "writer");

            WriteHandle(writer, command.FactionHandle);
            WriteLengthPrefixedHandleArray(writer, command.trainerHandles);
            WriteHandle(writer, command.traineePrototypeHandle);
            writer.Write((byte)command.traineeCount);
        }

        public static TrainCommand Deserialize(BinaryReader reader)
        {
            Argument.EnsureNotNull(reader, "reader");

            Handle factionHandle = ReadHandle(reader);
            var trainerHandles = ReadLengthPrefixedHandleArray(reader);
            Handle traineeTypeHandle = ReadHandle(reader);
            int traineeCount = reader.ReadByte();
            return new TrainCommand(factionHandle, trainerHandles, traineeTypeHandle, traineeCount);
        }
        #endregion
        #endregion
    }
}

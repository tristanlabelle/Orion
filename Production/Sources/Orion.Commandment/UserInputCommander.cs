using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.GameLogic;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Commandment
{
    /// <summary>
    /// A <see cref="Commander"/> which gives <see cref="Command"/>s based on user input.
    /// </summary>
    public sealed class UserInputCommander : Commander
    {
        #region Constructors
        /// <summary>
        /// Constructor For a commander that can listen input to create commands
        /// </summary>
        /// <param name="faction">the faction of the player.</param>
        public UserInputCommander(Faction faction)
            : base(faction)
        { }
        #endregion

        #region Methods
        public void CancelCommands(IEnumerable<Unit> units)
        {
            if (units.Count() > 0) GenerateCommand(new Cancel(Faction, units));
        }

        public void LaunchAttack(IEnumerable<Unit> units, Unit target)
        {
            if (units.Count() > 0) GenerateCommand(new Attack(Faction, units, target));
        }

        public void LaunchBuild(Unit builder, UnitType buildingType, Vector2 buildingPosition)
        {
            GenerateCommand(new Build(builder, buildingPosition, buildingType));
        }

        public void LaunchHarvest(IEnumerable<Unit> units, ResourceNode node)
        {
            if (units.Count() > 0) GenerateCommand(new Harvest(Faction, units, node));
        }

        public void LaunchMove(IEnumerable<Unit> units, Vector2 destination)
        {
            if (units.Count() > 0) GenerateCommand(new Move(Faction, units, destination));
        }

        public void LaunchRepair(IEnumerable<Unit> units, Unit repairedUnit)
        {
            if (units.Count() > 0) GenerateCommand(new Repair(Faction, units, repairedUnit));
        }

        public void LaunchTrain(IEnumerable<Unit> buildings, UnitType trainedType)
        {
            if (buildings.Count() > 0) GenerateCommand(new Train(buildings, trainedType, Faction));
        }

        public void LaunchZoneAttack(IEnumerable<Unit> units, Vector2 destination)
        {
            if (units.Count() > 0) GenerateCommand(new ZoneAttack(Faction, units, destination));
        }

        public override void Update(float timeDelta)
        {
            (commandsEntryPoint as CommandSink).Flush();
        }

        public override void AddToPipeline(CommandPipeline pipeline)
        {
            pipeline.AddCommander(this);

            commandsEntryPoint = new CommandOptimizer(pipeline.UserCommandmentEntryPoint);
            commandsEntryPoint = new CommandAggregator(commandsEntryPoint);
        }
        #endregion
    }
}

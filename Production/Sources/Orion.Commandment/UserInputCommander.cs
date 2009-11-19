using System.Collections.Generic;
using System.Linq;
using OpenTK.Math;
using Orion.Commandment.Commands;
using Orion.Commandment.Pipeline;
using Orion.GameLogic;
using Keys = System.Windows.Forms.Keys;

namespace Orion.Commandment
{
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

        public void LaunchSuicide(IEnumerable<Unit> units)
        {
            if (units.Count() > 0) GenerateCommand(new Suicide(Faction, units));
        }

        public void LaunchChangeDimplomacy(Faction otherFaction)
        {
            if (otherFaction == null) return;
            if(Faction.AlliesID.Contains(otherFaction.ID)) GenerateCommand(new ChangeDiplomacy(Faction,otherFaction.ID,DiplomaticStance.Enemy));
            else GenerateCommand(new ChangeDiplomacy(Faction, otherFaction.ID, DiplomaticStance.Ally));
        }

        public void LaunchZoneAttack(IEnumerable<Unit> units, Vector2 destination)
        {
            if (units.Count() > 0) GenerateCommand(new ZoneAttack(Faction, units, destination));
        }
        #endregion
    }
}
